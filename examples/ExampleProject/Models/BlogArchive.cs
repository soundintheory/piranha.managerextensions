using Piranha.AttributeBuilder;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerExtensions.Lists.Attributes;

namespace ExampleProject.Models;

[PageType(Title = "Blog archive", UseBlocks = false, IsArchive = true)]
[PageTypeArchiveItem(typeof(BlogPost))]
[Singleton(Title = "Blog", ShowInMenu = true, Icon = "fa fa-bullhorn")]
[ContentTypeRoute(Title = "Default", Route = "/BlogArchive")]
public class BlogArchive : Page<BlogArchive>
{
}
