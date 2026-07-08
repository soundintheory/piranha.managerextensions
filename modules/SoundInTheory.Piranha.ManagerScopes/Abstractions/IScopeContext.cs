using System;

namespace SoundInTheory.Piranha.ManagerScopes.Abstractions;

/// <summary>
/// The manager user's currently selected scope, persisted across requests (session-backed by default).
/// Null means the unscoped interface. The page-tree root resolver reads this to re-root the tree.
/// </summary>
public interface IScopeContext
{
    /// <summary>The id of the scope page the manager is currently scoped to, or null when unscoped.</summary>
    Guid? CurrentScopeId { get; set; }
}
