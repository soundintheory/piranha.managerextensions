using System.Collections.Generic;
using System.Threading.Tasks;
using SoundInTheory.Piranha.ManagerScopes.Models;

namespace SoundInTheory.Piranha.ManagerScopes.Abstractions;

/// <summary>
/// Contributes items to the scoped manager navigation (the left-hand nav shown when a user is inside a
/// scope). Register one or more implementations; the aggregated items appear under the scope, in
/// addition to the built-in "Pages" item. Providers run per request while the scoped nav is built.
/// </summary>
public interface IScopedMenuItemProvider
{
    /// <summary>Returns the nav items this provider contributes for the current scope (may be empty).</summary>
    Task<IEnumerable<ScopedMenuItem>> GetMenuItemsAsync(ScopeMenuContext context);
}
