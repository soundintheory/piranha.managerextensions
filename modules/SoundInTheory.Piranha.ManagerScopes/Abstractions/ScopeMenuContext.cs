using System;
using System.Security.Claims;
using Piranha.Models;

namespace SoundInTheory.Piranha.ManagerScopes.Abstractions;

/// <summary>
/// Context passed to each <see cref="IScopedMenuItemProvider"/> when the scoped nav is built.
/// </summary>
public sealed class ScopeMenuContext
{
    /// <summary>The id of the current scope (the scope root page).</summary>
    public Guid ScopeId { get; init; }

    /// <summary>The current manager user.</summary>
    public ClaimsPrincipal User { get; init; }

    /// <summary>
    /// The loaded scope page (lightweight <see cref="PageInfo"/>). Carries <c>TypeId</c> and <c>Title</c>
    /// so a provider can inspect the page type (e.g. its regions) without loading it again.
    /// </summary>
    public PageInfo Page { get; init; }
}
