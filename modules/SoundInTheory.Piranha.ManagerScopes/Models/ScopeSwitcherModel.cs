using System;
using System.Collections.Generic;

namespace SoundInTheory.Piranha.ManagerScopes.Models;

/// <summary>Payload for the header scope switcher.</summary>
public sealed class ScopeSwitcherModel
{
    /// <summary>
    /// The scopes the current user may switch into. Declared as <see cref="IList{T}"/> (not
    /// IReadOnlyList) so Piranha's Newtonsoft config serializes it as a plain JSON array rather than a
    /// <c>{ $type, $values }</c> wrapper — the Vue switcher expects an array.
    /// </summary>
    public IList<ScopeInfo> Scopes { get; set; } = new List<ScopeInfo>();

    /// <summary>The user's current scope, or null when unscoped.</summary>
    public Guid? CurrentScopeId { get; set; }

    /// <summary>Whether the user may select the unscoped ("all content") option.</summary>
    public bool CanUnscoped { get; set; }
}
