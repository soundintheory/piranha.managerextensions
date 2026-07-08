using Piranha.AttributeBuilder;
using Piranha.Models;

namespace ExampleProject.Models;

[PageType(Title = "Generic page")]
[ContentTypeRoute(Title = "Default", Route = "/GenericPage")]
public class GenericPage  : Page<GenericPage>
{
}
