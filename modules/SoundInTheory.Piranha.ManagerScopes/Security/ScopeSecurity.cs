using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Piranha;
using Piranha.Models;
using Piranha.Security;
using SoundInTheory.Piranha.ManagerScopes.Models;
using SoundInTheory.Piranha.ManagerScopes.Services;

namespace SoundInTheory.Piranha.ManagerScopes.Security;

/// <summary>
/// Owns the security side of the module: it seeds the role-editor permissions (one per scope, plus the
/// "unscoped" permission), keeps them in sync as scope pages are created/renamed/deleted, and enforces
/// the per-scope claim on page save/delete — including saves that go through Piranha's own core API,
/// which the claim alone can't gate. Follows the dynamic-permissions pattern.
/// </summary>
public sealed class ScopeSecurity
{
    private static bool _hooksRegistered;
    private static readonly object HookLock = new();

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ManagerScopesOptions _options;

    public ScopeSecurity(IHttpContextAccessor httpContextAccessor, ManagerScopesOptions options)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    /// <summary>Seed the role-editor permissions from the scopes that exist at startup.</summary>
    public void SeedPermissions(IEnumerable<ScopeInfo> scopes)
    {
        EnsureUnscopedPermission();
        foreach (var scope in scopes)
        {
            EnsureScopePermission(scope.Id, scope.Title);
        }
    }

    /// <summary>Register the page hooks (idempotent — hooks are global/static).</summary>
    public void RegisterHooks()
    {
        lock (HookLock)
        {
            if (_hooksRegistered)
            {
                return;
            }
            _hooksRegistered = true;
        }

        // Keep the per-scope permission set in step with the scope pages.
        App.Hooks.Pages.RegisterOnAfterSave(page =>
        {
            if (IsScopeType(page.TypeId))
            {
                EnsureScopePermission(page.Id, page.Title);
            }
        });
        App.Hooks.Pages.RegisterOnAfterDelete(page =>
        {
            if (IsScopeType(page.TypeId))
            {
                RemoveScopePermission(page.Id);
            }
        });

        // Bind the per-scope claim to the resource on operations we don't own (core save/delete API).
        App.Hooks.Pages.RegisterOnBeforeSave(page => Enforce(page.Id, page.ParentId));
        App.Hooks.Pages.RegisterOnBeforeDelete(page => Enforce(page.Id, page.ParentId));
    }

    // ── Enforcement ──────────────────────────────────────────────────────────────────────────────

    private void Enforce(Guid pageId, Guid? parentId)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null)
        {
            // No request context (startup seed / background operation) — nothing to enforce against.
            return;
        }

        var scopeService = ctx.RequestServices.GetService<ScopeService>();
        if (scopeService == null)
        {
            return;
        }

        // sync-over-async is safe here: ASP.NET Core has no synchronization context, and the sitemap
        // is served from Piranha's cache after first load.
        var owningScope = scopeService.ResolveOwningScopeAsync(pageId, parentId).GetAwaiter().GetResult();

        var allowed = owningScope.HasValue
            ? scopeService.CanAccessScope(ctx.User, owningScope.Value)
            : scopeService.CanAccessUnscoped(ctx.User);

        if (!allowed)
        {
            throw new UnauthorizedAccessException("You do not have permission to modify content in this scope.");
        }
    }

    // ── Permission maintenance ─────────────────────────────────────────────────────────────────────

    private bool IsScopeType(string typeId) => typeId != null && _options.ScopeTypes.Contains(typeId);

    private static void EnsureUnscopedPermission()
    {
        var list = App.Permissions[ScopePermissions.Group];
        if (list.All(p => p.Name != ScopePermissions.Unscoped))
        {
            list.Add(new PermissionItem
            {
                Name = ScopePermissions.Unscoped,
                Title = "Use unscoped interface",
                Category = ScopePermissions.Category,
                IsInternal = false
            });
        }
    }

    private static void EnsureScopePermission(Guid scopeId, string title)
    {
        var name = ScopePermissions.ForScope(scopeId);
        var list = App.Permissions[ScopePermissions.Group];
        var existing = list.FirstOrDefault(p => p.Name == name);
        if (existing == null)
        {
            list.Add(new PermissionItem
            {
                Name = name,
                Title = title,
                Category = ScopePermissions.Category,
                IsInternal = false
            });
        }
        else
        {
            // Keep the checkbox label in sync when a scope page is renamed.
            existing.Title = title;
        }
    }

    private static void RemoveScopePermission(Guid scopeId)
    {
        var name = ScopePermissions.ForScope(scopeId);
        var list = App.Permissions[ScopePermissions.Group];
        var existing = list.FirstOrDefault(p => p.Name == name);
        if (existing != null)
        {
            list.Remove(existing);
        }
    }
}
