using System.Collections.Generic;
using System.Linq;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.ManagerScopes.Services;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

namespace SoundInTheory.Piranha.ManagerScopes.Filters;

/// <summary>
/// Shapes the unscoped page tree: shows scope roots the user can access, hides their sub-pages (so a
/// location's pages only appear once you switch into it), and hides other content from users who may
/// not use the unscoped interface. In the scoped view the tree is already re-rooted, so nothing is
/// filtered. Runs server-side, so hidden pages never reach the browser.
/// </summary>
public sealed class ScopePageTreeFilter : IPageTreeFilter
{
    private readonly IScopeContext _scope;
    private readonly ScopeService _scopes;

    public ScopePageTreeFilter(IScopeContext scope, ScopeService scopes)
    {
        _scope = scope;
        _scopes = scopes;
    }

    public bool Include(SitemapItem item, IReadOnlyList<SitemapItem> ancestors, PageTreeContext context)
    {
        // Scoped view: the tree is re-rooted at the scope — show its whole subtree.
        if (_scope.CurrentScopeId.HasValue)
        {
            return true;
        }

        // Unscoped view. A scope root is shown only if the user can access it.
        if (_scopes.IsScopeRoot(item))
        {
            return _scopes.CanAccessScope(context.User, item.Id);
        }

        // A scope's descendants are hidden from the unscoped tree (visible only inside the scope).
        if (ancestors.Any(a => _scopes.IsScopeRoot(a)))
        {
            return false;
        }

        // Non-scope content is shown only to users allowed the full, unscoped interface.
        return _scopes.CanAccessUnscoped(context.User);
    }
}
