using Piranha.AttributeBuilder;
using Piranha.Models;
using SoundInTheory.Piranha.ManagerExtensions.Lists.Attributes;

namespace ExampleProject.Models;

[PostType(Title = "Blog post")]
[ContentTypeRoute(Title = "Default", Route = "/BlogPost")]
public class BlogPost : Post<BlogPost>
{
}
