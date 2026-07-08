using System;

namespace SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

/// <summary>
/// Decides the root the page tree is rendered from. Register a custom implementation to derive the
/// root from request state (e.g. a session scope). The default returns the route-supplied root.
/// </summary>
public interface IPageTreeRootResolver
{
    /// <summary>
    /// Returns the id of the page the tree should be rooted at, or null for the full top-level tree.
    /// </summary>
    Guid? ResolveRoot(PageTreeContext context);
}
