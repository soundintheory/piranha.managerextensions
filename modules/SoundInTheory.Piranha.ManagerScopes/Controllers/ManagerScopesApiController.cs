using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager;
using SoundInTheory.Piranha.ManagerScopes.Abstractions;
using SoundInTheory.Piranha.ManagerScopes.Models;
using SoundInTheory.Piranha.ManagerScopes.Services;

namespace SoundInTheory.Piranha.ManagerScopes.Controllers;

/// <summary>
/// Backs the header scope switcher: lists the scopes the user may enter and sets the current scope in
/// session. Access to each scope (and to the unscoped option) is validated server-side.
/// </summary>
[Area("Manager")]
[Route("manager/api/managerscopes")]
[Authorize(Policy = Permission.Pages)]
[ApiController]
[AutoValidateAntiforgeryToken]
public sealed class ManagerScopesApiController : Controller
{
    private readonly ScopeService _scopes;
    private readonly IScopeContext _scopeContext;
    private readonly ScopeMenuService _menu;

    public ManagerScopesApiController(ScopeService scopes, IScopeContext scopeContext, ScopeMenuService menu)
    {
        _scopes = scopes;
        _scopeContext = scopeContext;
        _menu = menu;
    }

    /// <summary>The scopes available to the current user, plus the current selection.</summary>
    [HttpGet("list")]
    public async Task<ScopeSwitcherModel> List()
    {
        return new ScopeSwitcherModel
        {
            Scopes = (await _scopes.GetAccessibleScopesAsync(User)).ToList(),
            CurrentScopeId = _scopeContext.CurrentScopeId,
            CanUnscoped = _scopes.CanAccessUnscoped(User)
        };
    }

    /// <summary>
    /// Sets the current scope (omit the id to return to the unscoped interface). Rejected with 403 if
    /// the user may not access the target scope / the unscoped interface. <paramref name="active"/> is
    /// the InternalId of the menu item the user is currently on; the response's <c>redirect</c> is the
    /// equivalent item's route in the new scope, or the pages view when there's no equivalent.
    /// </summary>
    [HttpPost("set/{scopeId:guid?}")]
    public async Task<IActionResult> Set(Guid? scopeId = null, string active = null)
    {
        if (scopeId.HasValue)
        {
            if (!_scopes.CanAccessScope(User, scopeId.Value))
            {
                return Forbid();
            }
        }
        else if (!_scopes.CanAccessUnscoped(User))
        {
            return Forbid();
        }

        _scopeContext.CurrentScopeId = scopeId;

        // Compute the redirect server-side. GetMenuAsync reflects the scope we just set, so we can match
        // the active item's InternalId in the new scope's menu; otherwise fall back to the pages view.
        var redirect = Url.Content("~/manager/pages");
        if (!string.IsNullOrEmpty(active))
        {
            var menu = await _menu.GetMenuAsync(User);
            var match = menu?.Items.FirstOrDefault(i => i.InternalId == active);
            if (match != null)
            {
                redirect = Url.Content(match.Route);
            }
        }

        return Ok(new { scopeId, redirect });
    }
}
