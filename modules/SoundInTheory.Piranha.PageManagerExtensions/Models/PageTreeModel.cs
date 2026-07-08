using System;
using System.Collections.Generic;

namespace SoundInTheory.Piranha.PageManagerExtensions.Models;

/// <summary>
/// The manager page tree. Mirrors Piranha's <c>PageListModel</c> (a list of sites, each with its page
/// structure) so the same layout/components can be reused — with the added ability to re-root a site's
/// tree at a specific page and to prune it server-side (see <see cref="PageTreeService"/>).
/// </summary>
public sealed class PageTreeModel
{
    /// <summary>The sites shown. Normally every site; when re-rooted, a single re-rooted entry.</summary>
    public IList<PageTreeSite> Sites { get; set; } = new List<PageTreeSite>();

    /// <summary>Addable page types, for the "add page" picker.</summary>
    public IList<PageTreeType> PageTypes { get; set; } = new List<PageTreeType>();

    /// <summary>
    /// Whether <b>anything</b> is reorderable — used by the client to decide whether to wire up drag
    /// binding at all. Granularity is per sibling group (see <see cref="PageTreeNode.CanSort"/>).
    /// </summary>
    public bool CanReorder { get; set; }
}

/// <summary>One site column in the tree (or a single re-rooted view). Mirrors <c>PageListModel.PageSite</c>.</summary>
public sealed class PageTreeSite
{
    public Guid Id { get; set; }

    /// <summary>The site title, or — when re-rooted — the title of the root page.</summary>
    public string Title { get; set; }

    public string Slug { get; set; } = "/";

    /// <summary>Base manager site-edit route; the client appends the site id.</summary>
    public string EditUrl { get; set; } = "manager/site/edit/";

    /// <summary>
    /// When set, this view is re-rooted at the given page: <see cref="Title"/> is the root page's title
    /// and <see cref="Pages"/> are its children. Null for a normal, site-rooted view.
    /// </summary>
    public Guid? RootId { get; set; }

    /// <summary>
    /// Base manager edit route for the re-rooted header page; the client appends <see cref="RootId"/>.
    /// Defaults to the core page editor, but an <c>IPageTreeNodeCustomizer</c> can repoint it.
    /// </summary>
    public string RootEditUrl { get; set; } = "manager/page/edit/";

    /// <summary>
    /// Whether the <b>top-level</b> group (this site's root pages, or a re-rooted page's children) is
    /// complete — nothing pruned — so those pages can be reordered among themselves and can receive a
    /// dropped page. Deeper groups carry their own flags on each node.
    /// </summary>
    public bool CanReorder { get; set; }

    public IList<PageTreeNode> Pages { get; set; } = new List<PageTreeNode>();
}

/// <summary>A single node in the page tree. Mirrors the fields Piranha's <c>sitemap-item</c> needs.</summary>
public sealed class PageTreeNode
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string Title { get; set; }
    public string TypeName { get; set; }
    public string Permalink { get; set; }

    /// <summary>Base manager edit route ("manager/page/edit/"); the client appends the id.</summary>
    public string EditUrl { get; set; } = "manager/page/edit/";

    /// <summary>Formatted published date for the date column (empty when unpublished).</summary>
    public string Published { get; set; }

    /// <summary>Status badge text ("Draft"/"Unpublished"), or empty when published.</summary>
    public string Status { get; set; }

    public bool IsExpanded { get; set; }

    /// <summary>True when the page has front-end view permissions (renders a lock icon).</summary>
    public bool IsRestricted { get; set; }

    /// <summary>True when the page is a copy of another page (renders a "copy" badge).</summary>
    public bool IsCopy { get; set; }

    /// <summary>True when the published date is in the future (renders a "scheduled" badge).</summary>
    public bool IsScheduled { get; set; }

    /// <summary>True when the page has no published date (dims the row).</summary>
    public bool IsUnpublished { get; set; }

    /// <summary>
    /// Whether this node can be dragged — true only when its sibling group is complete (no sibling was
    /// pruned by a filter). A node with hidden siblings can't be meaningfully repositioned among them.
    /// </summary>
    public bool CanSort { get; set; }

    /// <summary>
    /// Whether a page can be dropped into this node's children (and its children reordered) — true only
    /// when its children group is complete. False if any child was pruned, since the drop position
    /// relative to the hidden children would be ambiguous.
    /// </summary>
    public bool CanReceive { get; set; }

    public IList<PageTreeNode> Items { get; set; } = new List<PageTreeNode>();
}

/// <summary>An addable page type for the "add page" picker. Mirrors <c>ContentTypeModel</c>.</summary>
public sealed class PageTreeType
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    /// <summary>Base manager add route ("manager/page/add/"); the client appends siteId/typeId.</summary>
    public string AddUrl { get; set; } = "manager/page/add/";
}
