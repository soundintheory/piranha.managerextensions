using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piranha;
using Piranha.Models;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;
using SoundInTheory.Piranha.PageManagerExtensions.Models;

namespace SoundInTheory.Piranha.PageManagerExtensions.Services;

/// <summary>
/// Builds the manager page tree from the sitemap, mirroring Piranha's own PageService.GetList (all
/// sites, drafts, expanded-levels config) but adding seams: an <see cref="IPageTreeRootResolver"/> that
/// can re-root the tree at a page, <see cref="IPageTreeFilter"/>s that prune it server-side, and
/// <see cref="IPageTreeNodeCustomizer"/>s that tweak each built node (e.g. its edit URL). The core page
/// edit/save/move/delete API is reused unchanged by the client.
/// </summary>
public sealed class PageTreeService
{
    private readonly IApi _api;
    private readonly IPageTreeRootResolver _rootResolver;
    private readonly IEnumerable<IPageTreeFilter> _filters;
    private readonly IEnumerable<IPageTreeNodeCustomizer> _customizers;

    public PageTreeService(IApi api, IPageTreeRootResolver rootResolver,
        IEnumerable<IPageTreeFilter> filters, IEnumerable<IPageTreeNodeCustomizer> customizers)
    {
        _api = api;
        _rootResolver = rootResolver;
        _filters = filters;
        _customizers = customizers;
    }

    public async Task<PageTreeModel> GetTreeAsync(PageTreeContext context)
    {
        var rootId = _rootResolver.ResolveRoot(context);
        var model = new PageTreeModel();

        foreach (var pt in App.PageTypes)
        {
            model.PageTypes.Add(new PageTreeType { Id = pt.Id, Title = pt.Title, Description = pt.Description });
        }

        var expandedLevels = GetExpandedLevels();

        if (rootId.HasValue)
        {
            // Re-rooted view: find the owning site + root page, show the root's children under a header
            // that represents the root page. Reorder still works — moves post the real parent (the root).
            var site = await FindOwningSiteAsync(rootId.Value);
            if (site != null)
            {
                model.Sites.Add(await BuildRootedSiteAsync(site, rootId.Value, context, expandedLevels));
            }
        }
        else
        {
            // Normal view: every site, default site first — same as Piranha's PageService.GetList.
            var sites = (await _api.Sites.GetAllAsync()).OrderByDescending(s => s.IsDefault);
            foreach (var site in sites)
            {
                model.Sites.Add(await BuildSiteAsync(site, context, expandedLevels));
            }
        }

        // The client wires up drag binding when anything at all is reorderable (a complete sibling group).
        model.CanReorder = model.Sites.Any(s => AnyCanSort(s.Pages));
        return model;
    }

    private async Task<PageTreeSite> BuildSiteAsync(Site site, PageTreeContext context, int expandedLevels)
    {
        var result = new PageTreeSite { Id = site.Id, Title = site.Title };
        var sitemap = await _api.Sites.GetSitemapAsync(site.Id, onlyPublished: false);
        var drafts = await _api.Pages.GetAllDraftsAsync(site.Id);

        var (pages, complete) = BuildGroup(sitemap, site.Id, context, new List<SitemapItem>(), drafts, 0, expandedLevels);
        result.Pages = pages;
        result.CanReorder = complete;   // whether the top-level group (site roots) is complete
        return result;
    }

    private async Task<PageTreeSite> BuildRootedSiteAsync(Site site, Guid rootId, PageTreeContext context, int expandedLevels)
    {
        var result = new PageTreeSite { Id = site.Id, Title = site.Title, RootId = rootId };
        var sitemap = await _api.Sites.GetSitemapAsync(site.Id, onlyPublished: false);
        var drafts = await _api.Pages.GetAllDraftsAsync(site.Id);

        var rootItem = Find(sitemap, rootId);
        if (rootItem != null)
        {
            // Build the root as a node (so customizers run on it too), then present its children under a
            // header representing the root page. RootEditUrl carries the (possibly customized) edit route.
            var rootNode = BuildNode(rootItem, site.Id, context, new List<SitemapItem>(), drafts, 0, expandedLevels);
            result.Title = rootNode.Title;
            result.RootEditUrl = rootNode.EditUrl;
            result.Pages = rootNode.Items;
            result.CanReorder = rootNode.CanReceive;
        }
        return result;
    }

    /// <summary>
    /// Builds one sibling group. Returns the mapped nodes and whether the group is <c>complete</c> (no
    /// sibling pruned by a filter). Each node's <see cref="PageTreeNode.CanSort"/> is set from the
    /// group's completeness; its <see cref="PageTreeNode.CanReceive"/> from its own children group.
    /// </summary>
    private (IList<PageTreeNode> nodes, bool complete) BuildGroup(IEnumerable<SitemapItem> items, Guid siteId,
        PageTreeContext context, List<SitemapItem> ancestors, IEnumerable<Guid> drafts, int level, int expandedLevels)
    {
        var nodes = new List<PageTreeNode>();
        var complete = true;

        foreach (var item in items)
        {
            // A filtered-out sibling means this group is no longer complete — its members can't be sorted.
            if (_filters.Any(f => !f.Include(item, ancestors, context)))
            {
                complete = false;
                continue;
            }
            nodes.Add(BuildNode(item, siteId, context, ancestors, drafts, level, expandedLevels));
        }

        foreach (var n in nodes)
        {
            n.CanSort = complete;   // a node is draggable only if its whole sibling group is present
        }
        return (nodes, complete);
    }

    /// <summary>Builds a single node (and its children group), then runs the node customizers.</summary>
    private PageTreeNode BuildNode(SitemapItem item, Guid siteId, PageTreeContext context,
        List<SitemapItem> ancestors, IEnumerable<Guid> drafts, int level, int expandedLevels)
    {
        var isDraft = drafts.Contains(item.Id);
        var node = new PageTreeNode
        {
            Id = item.Id,
            SiteId = siteId,
            Title = item.MenuTitle,
            TypeName = item.PageTypeName,
            Permalink = item.Permalink,
            Published = item.Published.HasValue ? item.Published.Value.ToString("yyyy-MM-dd") : null,
            Status = isDraft ? "Draft" : !item.Published.HasValue ? "Unpublished" : "",
            IsExpanded = level < expandedLevels,
            IsRestricted = item.Permissions.Count > 0,
            IsCopy = item.OriginalPageId.HasValue,
            IsScheduled = item.Published.HasValue && item.Published.Value > DateTime.Now,
            IsUnpublished = !item.Published.HasValue
        };

        ancestors.Add(item);
        var (childNodes, childComplete) = BuildGroup(item.Items, siteId, context, ancestors, drafts, level + 1, expandedLevels);
        ancestors.RemoveAt(ancestors.Count - 1);

        node.Items = childNodes;
        node.CanReceive = childComplete;   // may a page be dropped into this node's children?

        foreach (var customizer in _customizers)
        {
            customizer.Customize(node, item, context);
        }
        return node;
    }

    private static bool AnyCanSort(IEnumerable<PageTreeNode> nodes) =>
        nodes.Any(n => n.CanSort || AnyCanSort(n.Items));

    /// <summary>
    /// Moves a single page to sit under <paramref name="parentId"/> immediately after
    /// <paramref name="after"/> (or first when null). The absolute sort order is computed from the
    /// destination's real children — so it's correct even if some are hidden. The core page move hooks
    /// (incl. any access enforcement) run via <c>_api.Pages.MoveAsync</c>. Returns false if not found.
    /// </summary>
    public async Task<bool> MoveAsync(Guid id, Guid? parentId, Guid? after)
    {
        var page = await _api.Pages.GetByIdAsync<PageInfo>(id);
        if (page == null)
        {
            return false;
        }

        var sitemap = await _api.Sites.GetSitemapAsync(page.SiteId, onlyPublished: false);
        var group = parentId.HasValue
            ? (Find(sitemap, parentId.Value)?.Items ?? (IList<SitemapItem>)new List<SitemapItem>())
            : sitemap;

        // Siblings in their current order, excluding the page being moved.
        var siblings = group.Where(s => s.Id != id).ToList();

        int sortOrder;
        if (after.HasValue)
        {
            var index = siblings.FindIndex(s => s.Id == after.Value);
            sortOrder = index >= 0 ? index + 1 : siblings.Count;
        }
        else
        {
            sortOrder = 0;
        }

        await _api.Pages.MoveAsync(page, parentId, sortOrder);
        return true;
    }

    private async Task<Site> FindOwningSiteAsync(Guid pageId)
    {
        foreach (var site in await _api.Sites.GetAllAsync())
        {
            var sitemap = await _api.Sites.GetSitemapAsync(site.Id, onlyPublished: false);
            if (Find(sitemap, pageId) != null)
            {
                return site;
            }
        }
        return null;
    }

    private int GetExpandedLevels()
    {
        using var config = new Config(_api);
        return config.ManagerExpandedSitemapLevels;
    }

    private static SitemapItem Find(IEnumerable<SitemapItem> items, Guid id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
            {
                return item;
            }
            var found = Find(item.Items, id);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
