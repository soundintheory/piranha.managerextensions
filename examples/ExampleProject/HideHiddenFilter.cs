using System;
using System.Collections.Generic;
using Piranha.Models;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

namespace ExampleProject;

/// <summary>
/// Demo page-tree filter: hides any page whose title contains "Hidden" (and its whole subtree) from
/// the manager tree. Shows how a consumer plugs server-side filtering into PageManagerExtensions —
/// the same seam ManagerScopes will use to hide scope subtrees / inaccessible scopes.
/// </summary>
public sealed class HideHiddenFilter : IPageTreeFilter
{
    public bool Include(SitemapItem item, IReadOnlyList<SitemapItem> ancestors, PageTreeContext context)
        => string.IsNullOrEmpty(item.Title)
            || !item.Title.Contains("Hidden", StringComparison.OrdinalIgnoreCase);
}
