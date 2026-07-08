using Piranha.AttributeBuilder;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerExtensions.Lists.Attributes;

namespace ExampleProject.Models;

[PageType(Title = "Homepage")]
[Singleton(Title = "Homepage", ShowInMenu = false, Icon = "fa fa-home")]
[ContentTypeRoute(Title = "Default", Route = "/HomePage")]
public class HomePage : Page<GenericPage>
{
}
