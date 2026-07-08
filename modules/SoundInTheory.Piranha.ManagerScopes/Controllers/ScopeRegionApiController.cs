using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager;
using Piranha.Manager.Models.Content;
using Piranha.Manager.Services;

namespace SoundInTheory.Piranha.ManagerScopes.Controllers;

/// <summary>
/// Backs the single-region edit view: loads one region of a page and saves it back by <b>merging</b> it
/// into a freshly-loaded copy of the page. Loading fresh on save means edits to other regions (by other
/// users, or the same user in the full editor) are never clobbered — only the one region is written.
/// Reuses Piranha's manager <see cref="PageService"/> so the region/field transform is unchanged.
/// </summary>
[Area("Manager")]
[Route("manager/api/scoperegion")]
[Authorize(Policy = Permission.Pages)]
[ApiController]
[AutoValidateAntiforgeryToken]
public sealed class ScopeRegionApiController : Controller
{
    private readonly PageService _pages;

    public ScopeRegionApiController(PageService pages) => _pages = pages;

    /// <summary>Returns the edit model for a single region of the page (plus the page type + title).</summary>
    [HttpGet("{pageId:guid}/{regionId}")]
    public async Task<IActionResult> Get(Guid pageId, string regionId)
    {
        var model = await _pages.GetById(pageId);
        var region = model?.Regions.FirstOrDefault(r => r.Meta.Id == regionId);
        if (region == null)
        {
            return NotFound();
        }
        return Ok(new { typeId = model.TypeId, title = model.Title, region });
    }

    /// <summary>Merges the edited region into a fresh copy of the page and saves it.</summary>
    [HttpPost("{pageId:guid}/{regionId}")]
    public async Task<IActionResult> Save(Guid pageId, string regionId, [FromBody] RegionModel region)
    {
        if (region == null)
        {
            return BadRequest();
        }

        var model = await _pages.GetById(pageId);
        var index = model?.Regions.ToList().FindIndex(r => r.Meta.Id == regionId) ?? -1;
        if (index < 0)
        {
            return NotFound();
        }

        // Merge just this region into the freshly-loaded page, then save the whole (intact) page.
        model.Regions[index] = region;

        try
        {
            await _pages.Save(model, draft: false);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new { type = "danger", body = ex.Message });
        }

        return Ok(new { type = "success", body = "The region has been saved" });
    }
}
