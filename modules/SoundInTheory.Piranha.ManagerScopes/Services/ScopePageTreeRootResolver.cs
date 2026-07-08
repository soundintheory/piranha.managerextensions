using System;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

namespace SoundInTheory.Piranha.ManagerScopes.Services;

/// <summary>
/// Re-roots PageManagerExtensions' page tree at the current scope. Overrides the module's default
/// resolver (registered via TryAdd).
/// </summary>
/// <remarks>
/// Deliberately ignores <see cref="PageTreeContext.RequestedRootId"/> (the route-supplied root): when
/// ManagerScopes is active, the session scope is the single source of truth for the root and is subject
/// to access control (validated by the switcher). Honouring an arbitrary route root would bypass that —
/// and would also disagree with <c>ScopePageTreeFilter</c>, which prunes scope sub-trees based on the
/// session scope. So an unscoped session yields the normal multi-site view regardless of the route.
/// </remarks>
public sealed class ScopePageTreeRootResolver : IPageTreeRootResolver
{
    private readonly IScopeContext _scope;

    public ScopePageTreeRootResolver(IScopeContext scope)
    {
        _scope = scope;
    }

    public Guid? ResolveRoot(PageTreeContext context) => _scope.CurrentScopeId;
}
