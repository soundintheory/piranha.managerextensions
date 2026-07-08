using System.Collections.Generic;

namespace SoundInTheory.Piranha.ManagerScopes.Models;

/// <summary>A single item in the scoped manager navigation.</summary>
public sealed class ScopedMenuItem
{
    /// <summary>Stable id, used for active-state matching. Optional.</summary>
    public string InternalId { get; set; }

    /// <summary>The display text.</summary>
    public string Title { get; set; }

    /// <summary>The target route, app-relative (e.g. <c>~/manager/pages</c>).</summary>
    public string Route { get; set; }

    /// <summary>Font Awesome icon class (e.g. <c>fas fa-sitemap</c>).</summary>
    public string Css { get; set; } = "fas fa-circle";

    /// <summary>Ordering hint; items are shown ascending.</summary>
    public int SortOrder { get; set; }
}

/// <summary>The full scoped navigation for the current scope.</summary>
public sealed class ScopedMenu
{
    /// <summary>The scope's title, shown as the nav header.</summary>
    public string ScopeTitle { get; set; }

    /// <summary>The nav items (the built-in "Pages" item plus provider contributions), ordered.</summary>
    public IReadOnlyList<ScopedMenuItem> Items { get; set; }
}
