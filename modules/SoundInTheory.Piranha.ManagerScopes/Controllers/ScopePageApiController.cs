using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Piranha.Manager;
using Piranha.Manager.Models.Content;
using Piranha.Manager.Services;
using SoundInTheory.Piranha.ManagerScopes.Providers;
using SoundInTheory.Piranha.ManagerScopes.Services;

namespace SoundInTheory.Piranha.ManagerScopes.Controllers;

/// <summary>
/// Backs the bespoke scope-page editor. It edits a scope page's "main" content — everything <b>except</b>
/// the regions surfaced as scoped menu items (those are edited via the single-region screen). Saving
/// merges the posted title/regions into a freshly-loaded copy of the page, so the menu regions (and
/// anything else) are never clobbered.
/// </summary>
[Area("Manager")]
[Route("manager/api/scopepage")]
[Authorize(Policy = Permission.Pages)]
[ApiController]
[AutoValidateAntiforgeryToken]
public sealed class ScopePageApiController : Controller
{
    private readonly PageService _pages;
    private readonly ScopeMenuService _menu;

    public ScopePageApiController(PageService pages, ScopeMenuService menu)
    {
        _pages = pages;
        _menu = menu;
    }

    /// <summary>The page's title + the regions that are NOT surfaced in the scoped menu.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var model = await _pages.GetById(id);
        if (model == null)
        {
            return NotFound();
        }

        var omitted = await OmittedRegionIds(id);
        var regions = model.Regions.Where(r => !omitted.Contains(r.Meta.Id)).ToList();
        return Ok(new { typeId = model.TypeId, title = model.Title, useBlocks = model.UseBlocks, blocks = model.Blocks, regions });
    }

    /// <summary>Merges the posted title + regions into a fresh copy of the page and saves it.</summary>
    [HttpPost("{id:guid}")]
    public async Task<IActionResult> Save(Guid id, [FromBody] ScopePageSaveModel body)
    {
        if (body == null)
        {
            return BadRequest();
        }

        var model = await _pages.GetById(id);
        if (model == null)
        {
            return NotFound();
        }

        if (body.Title != null)
        {
            model.Title = body.Title;
        }

        // Blocks aren't split across menu items — the scope-page form owns them all, so replace wholesale
        // (only when the type uses blocks and the client actually sent them).
        if (model.UseBlocks && body.Blocks != null)
        {
            model.Blocks = body.Blocks;
        }

        foreach (var region in body.Regions ?? new List<RegionModel>())
        {
            for (var i = 0; i < model.Regions.Count; i++)
            {
                if (model.Regions[i].Meta.Id == region.Meta.Id)
                {
                    model.Regions[i] = region;
                    break;
                }
            }
        }

        try
        {
            await _pages.Save(model, draft: false);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Ok(new { type = "danger", body = ex.Message });
        }

        return Ok(new { type = "success", body = "The page has been saved" });
    }

    private async Task<HashSet<string>> OmittedRegionIds(Guid scopeId)
    {
        var ids = new HashSet<string>();
        var menu = await _menu.BuildMenuAsync(scopeId, User);
        if (menu != null)
        {
            foreach (var item in menu.Items.Where(i => i.InternalId != null &&
                     i.InternalId.StartsWith(RegionScopedMenuItemProvider.InternalIdPrefix, StringComparison.Ordinal)))
            {
                ids.Add(item.InternalId.Substring(RegionScopedMenuItemProvider.InternalIdPrefix.Length));
            }
        }
        return ids;
    }
}

/// <summary>The payload posted by the scope-page editor: the title, blocks, and (non-menu) regions.</summary>
public sealed class ScopePageSaveModel
{
    public string Title { get; set; }
    public IList<BlockModel> Blocks { get; set; }
    public IList<RegionModel> Regions { get; set; }
}
