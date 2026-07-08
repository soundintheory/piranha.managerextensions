using System.Collections.Generic;

namespace SoundInTheory.Piranha.ManagerScopes;

/// <summary>
/// Configures which pages act as switchable "scopes". A scope is any page whose <b>page-type id</b> is
/// listed in <see cref="ScopeTypes"/>; switching into it re-roots the manager page tree at that page
/// (see <see cref="ManagerScopesExtensions.AddManagerScopes"/>).
/// </summary>
/// <example>
/// <code>
/// options.AddManagerScopes(o => o.ScopeTypes.Add(nameof(LocationPage)));
/// </code>
/// </example>
public sealed class ManagerScopesOptions
{
    /// <summary>
    /// Page-type ids (e.g. the page class name, <c>PageType.Id</c>) whose pages are scope roots.
    /// Matched against the resolved page-type title on the sitemap, so ids are the stable input.
    /// </summary>
    public ICollection<string> ScopeTypes { get; } = new HashSet<string>();

    /// <summary>
    /// When true (default), users lacking the "unscoped" permission are forced into their first
    /// accessible scope instead of seeing the full, unscoped tree. Set false to let everyone use the
    /// unscoped interface (the per-scope permissions then only prune which scopes appear).
    /// </summary>
    public bool RequireUnscopedPermission { get; set; } = true;
}
