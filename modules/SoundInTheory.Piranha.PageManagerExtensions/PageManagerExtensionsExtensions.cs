using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Piranha;
using Piranha.AspNetCore;
using Piranha.Manager;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;
using SoundInTheory.Piranha.PageManagerExtensions.Services;

namespace SoundInTheory.Piranha.PageManagerExtensions;

/// <summary>
/// Startup wiring. Replaces Piranha's Pages screen with a configurable, rooted/filtered page tree.
///   services:  AddPiranha(o => { ...; o.UsePageManagerExtensions(); })
///   app:       app.UsePiranha(o => { ...; o.UsePageManagerExtensions(); })
/// </summary>
public static class PageManagerExtensionsExtensions
{
    /// <summary>Registers the module on the Piranha service builder.</summary>
    public static PiranhaServiceBuilder UsePageManagerExtensions(this PiranhaServiceBuilder builder)
    {
        builder.Services.AddPageManagerExtensions();
        return builder;
    }

    /// <summary>Registers the module, the Pages-screen route swap, and the tree services.</summary>
    public static IServiceCollection AddPageManagerExtensions(this IServiceCollection services)
    {
        App.Modules.Register<Module>();

        // Swap the core Pages screen for our replacement.
        services.AddRazorPages(o => o.Conventions.Add(new PageManagerRoutingConvention()));

        // Tree builder + the default root resolver. Consumers can override the resolver (TryAdd) and
        // register any number of IPageTreeFilter implementations to prune the tree server-side.
        services.AddScoped<PageTreeService>();
        services.TryAddScoped<IPageTreeRootResolver, DefaultPageTreeRootResolver>();

        // Shared component bundle (the replacement page loads its own pagemanager.js).
        App.Modules.Manager().Scripts.Add(new ManagerScriptDefinition(Module.AssetPath + "/vue/app.js"));

        return services;
    }

    /// <summary>Wires up the module on the Piranha application builder.</summary>
    public static PiranhaApplicationBuilder UsePageManagerExtensions(this PiranhaApplicationBuilder builder)
    {
        builder.Builder.UsePageManagerExtensions();
        return builder;
    }

    /// <summary>Serves the module's static assets at <see cref="Module.AssetPath"/>.</summary>
    public static IApplicationBuilder UsePageManagerExtensions(this IApplicationBuilder builder)
    {
        return builder.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = AssetFileProvider,
            RequestPath = Module.AssetPath
        });
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
