using Piranha.AttributeBuilder;
using Piranha.Extend;
using Piranha.Extend.Fields;
using Piranha.Models;

namespace ExampleProject.Models;

/// <summary>
/// Demo scope type. Pages of this type are registered as manager "scopes" (see Program.cs
/// <c>UseManagerScopes</c>). Its regions drive the scoped nav via RegionScopedMenuItemProvider.
/// </summary>
[PageType(Title = "Location")]
[ContentTypeRoute(Title = "Default", Route = "/LocationPage")]
public class LocationPage : Page<LocationPage>
{
    [Region(Title = "Hero", Icon = "fas fa-image")]
    public HtmlField Hero { get; set; }

    [Region(Title = "Address", Icon = "fas fa-map-marker-alt")]
    public TextField Address { get; set; }

    [Region(Title = "Phone", Icon = "fas fa-phone")]
    public StringField Phone { get; set; }
}
