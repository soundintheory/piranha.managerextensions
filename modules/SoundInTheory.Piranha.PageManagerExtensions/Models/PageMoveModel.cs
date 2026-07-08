using System;

namespace SoundInTheory.Piranha.PageManagerExtensions.Models;

/// <summary>
/// A single drag-move: place page <see cref="Id"/> under <see cref="ParentId"/> (null = a site root)
/// immediately after <see cref="After"/> (null = first). No full-tree structure is sent — only the one
/// moved node and its new neighbours, so pruned/hidden siblings are never involved.
/// </summary>
public sealed class PageMoveModel
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? After { get; set; }
}
