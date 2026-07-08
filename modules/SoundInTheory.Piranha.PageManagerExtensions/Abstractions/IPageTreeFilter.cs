using System.Collections.Generic;
using Piranha.Models;

namespace SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

/// <summary>
/// Filters nodes out of the page tree. Register one or more implementations; a node (and its whole
/// subtree) is shown only if <b>every</b> filter includes it. Filtering happens server-side while the
/// tree is built, so hidden pages never reach the browser.
/// </summary>
public interface IPageTreeFilter
{
    /// <summary>
    /// Return false to hide this sitemap node (and its descendants) from the tree.
    /// </summary>
    /// <param name="item">The node being considered.</param>
    /// <param name="ancestors">
    /// The node's ancestor chain from the tree root down to (but excluding) the node itself — nearest
    /// parent last, empty for a root. Lets a filter decide based on where the node sits, e.g. hide
    /// everything below a page of a given type without re-walking the tree.
    /// </param>
    /// <param name="context">The tree-build context (user, site, requested root).</param>
    bool Include(SitemapItem item, IReadOnlyList<SitemapItem> ancestors, PageTreeContext context);
}
