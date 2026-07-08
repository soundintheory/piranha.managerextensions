using Piranha.Models;
using SoundInTheory.Piranha.ManagerScopes.Services;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;
using SoundInTheory.Piranha.PageManagerExtensions.Models;

namespace SoundInTheory.Piranha.ManagerScopes.Providers;

/// <summary>
/// Points a scope page's edit link at the bespoke scope-page editor (which omits the regions surfaced
/// as scoped menu items). Applies wherever the scope page appears — as a tree node when unscoped, and as
/// the re-rooted header when scoped — via PageManagerExtensions' <see cref="IPageTreeNodeCustomizer"/>.
/// </summary>
public sealed class ScopePageEditUrlCustomizer : IPageTreeNodeCustomizer
{
    private readonly ScopeService _scopes;

    public ScopePageEditUrlCustomizer(ScopeService scopes) => _scopes = scopes;

    public void Customize(PageTreeNode node, SitemapItem item, PageTreeContext context)
    {
        if (_scopes.IsScopeRoot(item))
        {
            node.EditUrl = "manager/scopepage/";
        }
    }
}
