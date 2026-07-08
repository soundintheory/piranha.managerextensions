using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Piranha.Manager;

namespace SoundInTheory.Piranha.PageManagerExtensions.Areas.Manager.Pages.PageManager;

/// <summary>
/// The replacement Pages screen. Routed to <c>manager/pages/{rootId?}</c> by
/// <see cref="PageManagerRoutingConvention"/>; keeps the "Pages" menu item active via ViewBag.
/// </summary>
[Authorize(Policy = Permission.Pages)]
public class IndexModel : PageModel
{
    public Guid? RootId { get; private set; }

    public void OnGet(Guid? rootId = null) => RootId = rootId;
}
