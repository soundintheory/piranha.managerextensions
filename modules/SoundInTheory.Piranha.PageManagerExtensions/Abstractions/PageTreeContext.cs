using System;
using System.Security.Claims;

namespace SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

/// <summary>
/// Context passed to the root resolver and tree filters when a page tree is built. Carries the
/// current user and the optional root id requested via the route, so consumers can make
/// scope-/permission-aware decisions.
/// </summary>
public sealed class PageTreeContext
{
    /// <summary>The current manager user.</summary>
    public ClaimsPrincipal User { get; init; }

    /// <summary>The site whose tree is being built (null = the default site).</summary>
    public Guid? SiteId { get; init; }

    /// <summary>The root page id requested on the route, if any. A resolver may honour or override it.</summary>
    public Guid? RequestedRootId { get; init; }
}
