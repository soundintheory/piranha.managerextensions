using System;

namespace SoundInTheory.Piranha.ManagerScopes.Models;

/// <summary>A switchable scope (a page of a configured scope type), as shown in the header switcher.</summary>
public sealed class ScopeInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}
