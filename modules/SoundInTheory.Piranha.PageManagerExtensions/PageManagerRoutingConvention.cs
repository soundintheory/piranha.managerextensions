using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SoundInTheory.Piranha.PageManagerExtensions;

/// <summary>
/// Swaps Piranha's built-in Pages screen for this module's replacement: it unmaps core
/// <c>PageList.cshtml</c> from <c>/manager/pages</c> and retargets our page to that URL, so the menu
/// item and link are unchanged.
/// </summary>
public sealed class PageManagerRoutingConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        // Unmap the core Pages screen.
        if (model.RelativePath == "/Areas/Manager/Pages/PageList.cshtml" && model.Selectors.Count > 0)
        {
            model.Selectors.RemoveAt(0);
        }

        // Point our replacement at the URL the core screen used to own. {rootId?} lets a tree be
        // deep-linked to a sub-root; the root resolver may still override it.
        if (model.RelativePath == "/Areas/Manager/Pages/PageManager/Index.cshtml" && model.Selectors.Count > 0)
        {
            model.Selectors[0].AttributeRouteModel.Template = "manager/pages/{rootId?}";
        }
    }
}
