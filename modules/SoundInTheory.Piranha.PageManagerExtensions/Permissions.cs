namespace SoundInTheory.Piranha.PageManagerExtensions;

/// <summary>
/// Permission/claim names for this module. The value is the claim name; register a matching
/// authorization policy (see <see cref="PageManagerExtensionsExtensions.AddPageManagerExtensions"/>) and a
/// <see cref="Piranha.Security.PermissionItem"/> so it appears in the manager user editor.
/// </summary>
public static class Permissions
{
    public const string Default = "SoundInTheory.Piranha.PageManagerExtensions";
}
