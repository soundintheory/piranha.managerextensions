using Piranha.Models;
using SoundInTheory.Piranha.PageManagerExtensions.Models;

namespace SoundInTheory.Piranha.PageManagerExtensions.Abstractions;

/// <summary>
/// Customizes a page-tree node after it has been built — most usefully to point a node's
/// <see cref="PageTreeNode.EditUrl"/> at a bespoke edit screen, but any display field can be tweaked.
/// Register one or more implementations; each runs on every node (and on the re-rooted header page),
/// server-side, as the tree is built — alongside the root resolver and filters.
/// </summary>
public interface IPageTreeNodeCustomizer
{
    /// <summary>Mutate the built node for the given sitemap item / context.</summary>
    void Customize(PageTreeNode node, SitemapItem item, PageTreeContext context);
}
