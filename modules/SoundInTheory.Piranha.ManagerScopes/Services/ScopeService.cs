using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Piranha;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerScopes.Models;
using SoundInTheory.Piranha.ManagerScopes.Security;

namespace SoundInTheory.Piranha.ManagerScopes.Services;

/// <summary>
/// Resolves scopes (pages of a configured type) from the sitemap and answers the access questions the
/// resolver, filter, switcher and enforcement hooks all need. Scoped per request; the sitemap and the
/// resolved scope-type titles are loaded once and reused.
/// </summary>
public sealed class ScopeService
{
    private readonly IApi _api;
    private readonly ManagerScopesOptions _options;

    private Sitemap _sitemap;
    private HashSet<string> _scopeTitles;

    public ScopeService(IApi api, ManagerScopesOptions options)
    {
        _api = api;
        _options = options;
    }

    // ── Access checks (synchronous claim checks; admins bypass) ──────────────────────────────────

    /// <summary>Whether the user may access the given scope (admins always may).</summary>
    public bool CanAccessScope(ClaimsPrincipal user, Guid scopeId) =>
        IsAdmin(user) || HasClaim(user, ScopePermissions.ForScope(scopeId));

    /// <summary>
    /// Whether the user may use the full, unscoped interface. Always true when the option is disabled;
    /// otherwise admins and holders of the "unscoped" permission may.
    /// </summary>
    public bool CanAccessUnscoped(ClaimsPrincipal user) =>
        !_options.RequireUnscopedPermission || IsAdmin(user) || HasClaim(user, ScopePermissions.Unscoped);

    // ── Scope-type test (synchronous; used per-node by the tree filter) ──────────────────────────

    /// <summary>True if the sitemap node is a scope root (its page type is a configured scope type).</summary>
    public bool IsScopeRoot(SitemapItem item) =>
        item?.PageTypeName != null && ScopeTitles.Contains(item.PageTypeName);

    // ── Scope enumeration (async; loads the sitemap) ─────────────────────────────────────────────

    /// <summary>All scopes on the default site, regardless of access.</summary>
    public async Task<IReadOnlyList<ScopeInfo>> GetScopesAsync()
    {
        var sitemap = await GetSitemapAsync();
        var scopes = new List<ScopeInfo>();
        Collect(sitemap, scopes);
        return scopes;
    }

    /// <summary>The scopes the given user may access.</summary>
    public async Task<IReadOnlyList<ScopeInfo>> GetAccessibleScopesAsync(ClaimsPrincipal user)
    {
        var scopes = await GetScopesAsync();
        return scopes.Where(s => CanAccessScope(user, s.Id)).ToList();
    }

    // ── Owning-scope resolution (async; used by enforcement hooks) ───────────────────────────────

    /// <summary>
    /// The nearest scope root at or above the given page (walking parents), or null if the page sits
    /// outside every scope. <paramref name="parentIdFallback"/> is used when the page isn't in the
    /// sitemap yet (e.g. a brand-new page being saved).
    /// </summary>
    public async Task<Guid?> ResolveOwningScopeAsync(Guid pageId, Guid? parentIdFallback = null)
    {
        var sitemap = await GetSitemapAsync();
        var byId = new Dictionary<Guid, SitemapItem>();
        Flatten(sitemap, byId);

        SitemapItem current = byId.TryGetValue(pageId, out var self)
            ? self
            : (parentIdFallback.HasValue && byId.TryGetValue(parentIdFallback.Value, out var parent) ? parent : null);

        while (current != null)
        {
            if (IsScopeRoot(current))
            {
                return current.Id;
            }
            current = current.ParentId.HasValue && byId.TryGetValue(current.ParentId.Value, out var next) ? next : null;
        }
        return null;
    }

    // ── Internals ────────────────────────────────────────────────────────────────────────────────

    private HashSet<string> ScopeTitles =>
        _scopeTitles ??= App.PageTypes
            .Where(pt => _options.ScopeTypes.Contains(pt.Id))
            .Select(pt => pt.Title)
            .ToHashSet();

    private async Task<Sitemap> GetSitemapAsync()
    {
        if (_sitemap == null)
        {
            var site = await _api.Sites.GetDefaultAsync();
            _sitemap = site != null
                ? await _api.Sites.GetSitemapAsync(site.Id, onlyPublished: false)
                : new Sitemap();
        }
        return _sitemap;
    }

    private void Collect(IEnumerable<SitemapItem> items, List<ScopeInfo> scopes)
    {
        foreach (var item in items)
        {
            if (IsScopeRoot(item))
            {
                scopes.Add(new ScopeInfo { Id = item.Id, Title = item.Title });
            }
            Collect(item.Items, scopes);
        }
    }

    private static void Flatten(IEnumerable<SitemapItem> items, Dictionary<Guid, SitemapItem> byId)
    {
        foreach (var item in items)
        {
            byId[item.Id] = item;
            Flatten(item.Items, byId);
        }
    }

    private static bool IsAdmin(ClaimsPrincipal user) =>
        HasClaim(user, global::Piranha.Manager.Permission.Admin);

    private static bool HasClaim(ClaimsPrincipal user, string type) =>
        user != null && user.HasClaim(c => c.Type == type);
}
