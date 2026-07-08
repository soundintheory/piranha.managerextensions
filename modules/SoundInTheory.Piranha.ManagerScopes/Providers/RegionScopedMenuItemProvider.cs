using System.Collections.Generic;
using System.Threading.Tasks;
using Piranha;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.ManagerScopes.Models;

namespace SoundInTheory.Piranha.ManagerScopes.Providers;

/// <summary>
/// The default scoped-menu provider: one nav item per region defined on the current scope page's type.
/// Lets an editor jump straight to a location's "Hero", "Contact details", etc.
/// </summary>
/// <remarks>
/// Interim target: each region item opens the scope page's standard editor. A future single-region edit
/// view will replace <see cref="RegionRoute"/> — override it (or this provider) once that view exists.
/// </remarks>
public class RegionScopedMenuItemProvider : IScopedMenuItemProvider
{
    /// <summary>Prefix of the InternalId given to each region item (InternalId = prefix + region id).</summary>
    public const string InternalIdPrefix = "Region_";

    public Task<IEnumerable<ScopedMenuItem>> GetMenuItemsAsync(ScopeMenuContext context)
    {
        var items = new List<ScopedMenuItem>();
        var type = App.PageTypes.GetById(context.Page.TypeId);

        if (type != null)
        {
            var order = 10;
            foreach (var region in type.Regions)
            {
                items.Add(new ScopedMenuItem
                {
                    InternalId = InternalIdPrefix + region.Id,
                    Title = string.IsNullOrEmpty(region.Title) ? region.Id : region.Title,
                    Route = RegionRoute(context.ScopeId, region.Id),
                    Css = string.IsNullOrEmpty(region.Icon) ? "fas fa-puzzle-piece" : region.Icon,
                    SortOrder = order++
                });
            }
        }

        return Task.FromResult<IEnumerable<ScopedMenuItem>>(items);
    }

    /// <summary>
    /// The route a region item links to — the single-region edit screen. Override to point elsewhere.
    /// </summary>
    protected virtual string RegionRoute(System.Guid scopeId, string regionId) => $"~/manager/scoperegion/{scopeId}/{regionId}";
}
