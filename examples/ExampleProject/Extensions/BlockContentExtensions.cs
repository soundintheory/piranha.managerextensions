using Piranha.Extend;
using Piranha.Models;

namespace ExampleProject.Extensions
{
    public static class BlockContentExtensions
    {
        public static bool HasBlock<T>(this IBlockContent content) where T : Block
        {
            return content?.Blocks != null && content.Blocks.Any(b => b is T);
        }

        public static bool HasBlockAtIndex<T>(this IBlockContent content, int index) where T : Block
        {
            return content?.Blocks != null && content.Blocks.Count > index && content.Blocks[index] is T;
        }
    }
}
