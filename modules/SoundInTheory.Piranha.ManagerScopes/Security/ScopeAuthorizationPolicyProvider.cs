using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SoundInTheory.Piranha.ManagerScopes.Security;

/// <summary>
/// Supplies an authorization policy on demand for the runtime-growing set of per-scope permissions
/// (<c>Scope_{id}</c>). Anything else falls through to the default provider. Preferred over mutating
/// <see cref="AuthorizationOptions"/> after startup, which isn't thread-safe (see dynamic-permissions).
/// </summary>
public sealed class ScopeAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public ScopeAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (ScopePermissions.IsScopePermission(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireClaim(policyName, policyName)
                .Build();
            return Task.FromResult(policy);
        }
        return _fallback.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
}
