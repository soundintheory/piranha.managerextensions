using System;
using Microsoft.AspNetCore.Http;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;

namespace SoundInTheory.Piranha.ManagerScopes.Services;

/// <summary>
/// Session-backed <see cref="IScopeContext"/>. Requires session middleware — the module's
/// <c>UseManagerScopes</c> adds <c>UseSession()</c> so the manager has a session store.
/// </summary>
public sealed class SessionScopeContext : IScopeContext
{
    private const string SessionKey = "ManagerScopes.CurrentScope";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionScopeContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? CurrentScopeId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
            return Guid.TryParse(value, out var id) ? id : (Guid?)null;
        }
        set
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }
            if (value.HasValue)
            {
                session.SetString(SessionKey, value.Value.ToString());
            }
            else
            {
                session.Remove(SessionKey);
            }
        }
    }
}
