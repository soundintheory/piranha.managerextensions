using System.Linq;
using Piranha;
using Piranha.Extend;

namespace SoundInTheory.Piranha.ManagerScopes;

/// <summary>
/// The module entry point. Registered with Piranha via App.Modules.Register&lt;Module&gt;()
/// (see <see cref="ManagerScopesExtensions.AddManagerScopes"/>). Runtime wiring (permission seeding,
/// hooks) is done in the Use extension where the API and page types are available.
/// </summary>
public class Module : IModule
{
    /// <summary>
    /// The base URL this module's static assets are served from. Derived from the assembly name
    /// (e.g. assembly "SoundInTheory.Piranha.ManagerScopes" => "/manager/managerscopes/assets").
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
    public string Description => "Scopes the Piranha manager to a page sub-tree, with a header switcher and per-scope permissions.";

    /// <summary>Gets the module package url.</summary>
    public string PackageUrl => "";

    /// <summary>Gets the module icon url.</summary>
    public string IconUrl => "";

    /// <summary>Runs once during App.Init(). Nothing static to register here.</summary>
    public void Init()
    {
    }
}
