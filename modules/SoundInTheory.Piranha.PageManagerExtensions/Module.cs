using System.Linq;
using Piranha;
using Piranha.Extend;

namespace SoundInTheory.Piranha.PageManagerExtensions;

/// <summary>
/// The module entry point. Registered with Piranha via App.Modules.Register&lt;Module&gt;()
/// (see <see cref="PageManagerExtensionsExtensions.AddPageManagerExtensions"/>). Use Init() to register
/// fields, blocks, menu items, hooks, etc.
/// </summary>
public class Module : IModule
{
    /// <summary>
    /// The base URL this module's static assets are served from. Derived from the assembly name
    /// (e.g. assembly "Acme.Piranha.Widgets" => "/manager/widgets/assets").
    /// </summary>
    public static string AssetPath { get; } =
        "/manager/" + (typeof(Module).Assembly.GetName().Name ?? "module").Split('.').Last().ToLowerInvariant() + "/assets";

    /// <summary>Gets the module author.</summary>
    public string Author => "Sound in Theory";

    /// <summary>Gets the module name.</summary>
    public string Name => typeof(Module).Assembly.GetName().Name;

    /// <summary>Gets the module version (read from the assembly).</summary>
    public string Version => Utils.GetAssemblyVersion(typeof(Module).Assembly);

    /// <summary>Gets the module description.</summary>
    public string Description => "A Piranha CMS manager extension module.";

    /// <summary>Gets the module package url.</summary>
    public string PackageUrl => "";

    /// <summary>Gets the module icon url.</summary>
    public string IconUrl => "";

    /// <summary>
    /// Runs once during App.Init(). Register fields/blocks/menu items/hooks here.
    /// </summary>
    public void Init()
    {
    }
}
