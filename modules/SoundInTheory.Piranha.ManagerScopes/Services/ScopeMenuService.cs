using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Piranha;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.ManagerScopes.Models;

namespace SoundInTheory.Piranha.ManagerScopes.Services;

/// <summary>
/// Builds the scoped manager navigation for the current scope: a built-in "Pages" item plus everything
/// contributed by the registered <see cref="IScopedMenuItemProvider"/>s. Returns null when not scoped.
/// </summary>
public sealed class ScopeMenuService
{
    private readonly IScopeContext _scope;
    private readonly IApi _api;
    private readonly IEnumerable<IScopedMenuItemProvider> _providers;

    public ScopeMenuService(IScopeContext scope, IApi api, IEnumerable<IScopedMenuItemProvider> providers)
    {
        _scope = scope;
        _api = api;
        _providers = providers;
    }

    /// <summary>The scoped menu for the current session scope, or null when unscoped.</summary>
    public Task<ScopedMenu> GetMenuAsync(ClaimsPrincipal user)
    {
        var scopeId = _scope.CurrentScopeId;
        return scopeId.HasValue ? BuildMenuAsync(scopeId.Value, user) : Task.FromResult<ScopedMenu>(null);
    }

    /// <summary>
    /// The scoped menu for a specific scope, regardless of the session scope. Lets a bespoke edit screen
    /// discover which items (e.g. regions) the scoped nav surfaces for that page. Null if not found.
    /// </summary>
    public async Task<ScopedMenu> BuildMenuAsync(Guid scopeId, ClaimsPrincipal user)
    {
        var page = await _api.Pages.GetByIdAsync<PageInfo>(scopeId);
        if (page == null)
        {
            return null;
        }

        var context = new ScopeMenuContext { ScopeId = scopeId, User = user, Page = page };

        // The scoped tree ("Pages") is always available; providers add to it.
        var items = new List<ScopedMenuItem>
        {
            new ScopedMenuItem { InternalId = "Pages", Title = "Pages", Route = "~/manager/pages", Css = "fas fa-sitemap", SortOrder = 0 }
        };

        foreach (var provider in _providers)
        {
            var provided = await provider.GetMenuItemsAsync(context);
            if (provided != null)
            {
                items.AddRange(provided);
            }
        }

        return new ScopedMenu
        {
            ScopeTitle = page.Title,
            Items = items.OrderBy(i => i.SortOrder).ToList()
        };
    }
}
