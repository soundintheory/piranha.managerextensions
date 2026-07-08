using Piranha.Extend.Fields;
using Piranha.Models;

namespace ExampleProject.Extensions
{
    public static class PiranhaContentExtensions
    {
        public static bool HasPrimaryImage(this GenericContent content)
        {
            return !content.PrimaryImage.IsNullOrEmpty();
        }

        public static bool HasPrimaryImage(this RoutedContentBase content)
        {
            return !content.PrimaryImage.IsNullOrEmpty();
        }
    }
}
