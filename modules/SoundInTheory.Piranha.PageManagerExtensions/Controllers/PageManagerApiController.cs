using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager;
using SoundInTheory.Piranha.PageManagerExtensions.Abstractions;
using SoundInTheory.Piranha.PageManagerExtensions.Models;
using SoundInTheory.Piranha.PageManagerExtensions.Services;

namespace SoundInTheory.Piranha.PageManagerExtensions.Controllers;

/// <summary>
/// Serves the rooted/filtered page tree to the replacement Pages screen, plus a single-page move that
/// doesn't require the whole tree to be serialised (so filtered/re-rooted trees can still be reordered).
/// Other page mutations (save, delete, create, copy) continue to use Piranha's core
/// <c>manager/api/page</c> controller.
/// </summary>
[Area("Manager")]
[Route("manager/api/pagemanager")]
[Authorize(Policy = Permission.Pages)]
[ApiController]
[AutoValidateAntiforgeryToken]
public sealed class PageManagerApiController : Controller
{
    private readonly PageTreeService _service;
    private readonly ManagerLocalizer _localizer;

    public PageManagerApiController(PageTreeService service, ManagerLocalizer localizer)
    {
        _service = service;
        _localizer = localizer;
    }

    [HttpGet("list/{rootId:guid?}")]
    public Task<PageTreeModel> List(Guid? rootId = null)
    {
        return _service.GetTreeAsync(new PageTreeContext
        {
            User = User,
            RequestedRootId = rootId
        });
    }

    /// <summary>
    /// Moves a single page (id + new parent + the sibling it now follows). Returns a status message in
    /// the shape the client pushes to the notification hub. Access-enforcement hooks may reject the move.
    /// </summary>
    [HttpPost("move")]
    public async Task<IActionResult> Move([FromBody] PageMoveModel model)
    {
        if (model == null)
        {
            return BadRequest();
        }

        try
        {
            var moved = await _service.MoveAsync(model.Id, model.ParentId, model.After);
            return Ok(new
            {
                status = moved
                    ? new { type = "success", body = _localizer.Page["The page has been moved"].Value }
                    : new { type = "danger", body = _localizer.General["An error occured"].Value }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new { status = new { type = "danger", body = ex.Message } });
        }
    }
}
