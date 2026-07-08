using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piranha.Manager;

namespace SoundInTheory.Piranha.ManagerScopes.Areas.Manager.Pages.ScopeRegion;

/// <summary>
/// The single-region edit screen. Routed to <c>manager/scoperegion/{pageId}/{regionId}</c>; keeps the
/// "Pages" menu item active.
/// </summary>
[Authorize(Policy = Permission.Pages)]
public class IndexModel : PageModel
{
    public Guid PageId { get; private set; }
    public string RegionId { get; private set; }

    public void OnGet(Guid pageId, string regionId)
    {
        PageId = pageId;
        RegionId = regionId;
    }
}
