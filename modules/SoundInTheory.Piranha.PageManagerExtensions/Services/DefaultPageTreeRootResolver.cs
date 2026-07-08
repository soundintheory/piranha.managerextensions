using System;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

namespace SoundInTheory.Piranha.PageManagerExtensions.Services;

/// <summary>
/// Default resolver — uses the root supplied on the route (or null for the full tree). Registered with
/// TryAdd so a consumer (e.g. a scoping module) can override it to derive the root from request state.
/// </summary>
public sealed class DefaultPageTreeRootResolver : IPageTreeRootResolver
{
    public Guid? ResolveRoot(PageTreeContext context) => context.RequestedRootId;
}
