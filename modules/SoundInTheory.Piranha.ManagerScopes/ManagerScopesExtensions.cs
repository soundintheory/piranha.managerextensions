using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Piranha;
using Piranha.AspNetCore;
using Piranha.Manager;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.ManagerScopes.Filters;
using SoundInTheory.Piranha.ManagerScopes.Providers;
using SoundInTheory.Piranha.ManagerScopes.Security;
using SoundInTheory.Piranha.ManagerScopes.Services;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

namespace SoundInTheory.Piranha.ManagerScopes;

/// <summary>
/// Startup wiring. Scopes the manager to a configured page sub-tree with a header switcher and
/// per-scope permissions. <b>Requires PageManagerExtensions</b> (this module overrides its page-tree
/// root resolver and adds a filter):
///   services:  AddPiranha(o => { ...; o.UsePageManagerExtensions(); o.UseManagerScopes(c => c.ScopeTypes.Add("LocationPage")); })
///   app:       app.UsePiranha(o => { ...; o.UsePageManagerExtensions(); o.UseManagerScopes(); })
/// </summary>
public static class ManagerScopesExtensions
{
    /// <summary>Registers the module on the Piranha service builder.</summary>
    public static PiranhaServiceBuilder UseManagerScopes(this PiranhaServiceBuilder builder, Action<ManagerScopesOptions> configure = null)
    {
        builder.Services.AddManagerScopes(configure);
        return builder;
    }

    /// <summary>
    /// Registers the module, the session-backed scope context, the scope-aware page-tree root resolver
    /// and filter, the dynamic per-scope authorization policy provider, and the switcher UI assets.
    /// </summary>
    public static IServiceCollection AddManagerScopes(this IServiceCollection services, Action<ManagerScopesOptions> configure = null)
    {
        App.Modules.Register<Module>();

        var options = new ManagerScopesOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // The current scope lives in session (Piranha registers AddSession services; UseManagerScopes
        // adds the middleware). IHttpContextAccessor backs both the context and the enforcement hooks.
        services.AddHttpContextAccessor();
        services.AddSession();
        services.AddScoped<IScopeContext, SessionScopeContext>();
        services.AddScoped<ScopeService>();
        services.AddSingleton<ScopeSecurity>();

        // Override PageManagerExtensions' default root resolver (re-root at the current scope) and add
        // the scope filter alongside any others.
        services.AddScoped<IPageTreeRootResolver, ScopePageTreeRootResolver>();
        services.AddScoped<IPageTreeFilter, ScopePageTreeFilter>();

        // The per-scope permission set grows at runtime, so synthesize policies on demand.
        services.AddSingleton<IAuthorizationPolicyProvider, ScopeAuthorizationPolicyProvider>();

        // Scoped left-hand navigation: aggregates IScopedMenuItemProvider contributions. The default
        // provider adds an item per region on the scope page. Consumers can register more providers.
        services.AddScoped<ScopeMenuService>();
        services.AddScoped<IScopedMenuItemProvider, RegionScopedMenuItemProvider>();

        // Route a scope page's edit link at the bespoke scope-page editor (omits the menu regions).
        services.AddScoped<IPageTreeNodeCustomizer, ScopePageEditUrlCustomizer>();

        // Our replacement nav (always rendered; hides the core nav). It includes the scope switcher
        // markup in-flow; scopeswitcher.js mounts the Vue instance on it.
        var manager = App.Modules.Manager();
        manager.Partials.Add("/Areas/Manager/Partial/_ScopeNav.cshtml");
        manager.Scripts.Add(new ManagerScriptDefinition(Module.AssetPath + "/vue/scopeswitcher.js"));

        return services;
    }

    /// <summary>Wires up the module on the Piranha application builder.</summary>
    public static PiranhaApplicationBuilder UseManagerScopes(this PiranhaApplicationBuilder builder)
    {
        builder.Builder.UseManagerScopes();
        return builder;
    }

    /// <summary>
    /// Adds session middleware, serves the module assets, and (now that App.Init has run) seeds the
    /// per-scope role-editor permissions and registers the maintenance/enforcement hooks.
    /// </summary>
    public static IApplicationBuilder UseManagerScopes(this IApplicationBuilder builder)
    {
        // SessionScopeContext needs the session store; Piranha registers the services but not the
        // middleware, so add it here (before the manager endpoints run).
        builder.UseSession();

        builder.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = AssetFileProvider,
            RequestPath = Module.AssetPath
        });

        using (var scope = builder.ApplicationServices.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var scopeService = sp.GetRequiredService<ScopeService>();
            var security = sp.GetRequiredService<ScopeSecurity>();

            var scopes = scopeService.GetScopesAsync().GetAwaiter().GetResult();
            security.SeedPermissions(scopes);
            security.RegisterHooks();
        }

        return builder;
    }

    /// <summary>
    /// On a Debug build, serve assets from the source 'assets' folder (so `npm run watch` + refresh
    /// works with no recompile); otherwise serve the assets embedded in the assembly.
    /// </summary>
    private static IFileProvider AssetFileProvider
    {
        get
        {
#if DEBUG
            var sourceAssets = Path.Combine(Path.GetDirectoryName(GetThisFilePath()), "assets");
            if (Directory.Exists(sourceAssets))
            {
                return new PhysicalFileProvider(sourceAssets);
            }
#endif
            return new EmbeddedFileProvider(
                typeof(Module).Assembly,
                typeof(Module).Assembly.GetName().Name + ".assets");
        }
    }

    private static string GetThisFilePath([CallerFilePath] string path = null) => path;
}
