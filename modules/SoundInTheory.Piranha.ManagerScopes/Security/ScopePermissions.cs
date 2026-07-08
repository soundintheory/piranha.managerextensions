using System;

namespace SoundInTheory.Piranha.ManagerScopes.Security;

/// <summary>
/// Permission/claim names for scope access. Each name doubles as the claim type and the authorization
/// policy name. The per-scope set is data-driven (one per scope page) — see
/// <see cref="ScopeAuthorizationPolicyProvider"/> and the dynamic-permissions pattern.
/// </summary>
public static class ScopePermissions
{
    /// <summary>Group key under which the checkboxes appear in the role editor.</summary>
    public const string Group = "ManagerScopes";

    /// <summary>Category (sub-grouping) shown in the role editor.</summary>
    public const string Category = "Manager Scopes";

    /// <summary>Claim/policy granting use of the full, unscoped manager interface.</summary>
    public const string Unscoped = "ManagerScopes_Unscoped";

    /// <summary>Prefix for the per-scope claim/policy names.</summary>
    public const string ScopePrefix = "Scope_";

    /// <summary>The claim/policy name that grants access to a single scope.</summary>
    public static string ForScope(Guid scopeId) => ScopePrefix + scopeId;

    /// <summary>True if the given policy/claim name is a per-scope permission.</summary>
    public static bool IsScopePermission(string name) =>
        !string.IsNullOrEmpty(name) && name.StartsWith(ScopePrefix, StringComparison.Ordinal);
}
