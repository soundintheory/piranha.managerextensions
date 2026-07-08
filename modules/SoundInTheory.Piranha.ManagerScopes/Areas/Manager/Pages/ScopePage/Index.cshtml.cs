using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piranha.Manager;

namespace SoundInTheory.Piranha.ManagerScopes.Areas.Manager.Pages.ScopePage;

/// <summary>
/// The bespoke scope-page edit screen. Routed to <c>manager/scopepage/{id}</c> (a scope page's edit link
/// is pointed here by <c>ScopePageEditUrlCustomizer</c>); keeps the "Pages" menu item active.
/// </summary>
[Authorize(Policy = Permission.Pages)]
public class IndexModel : PageModel
{
    public Guid PageId { get; private set; }

    public void OnGet(Guid id) => PageId = id;
}
