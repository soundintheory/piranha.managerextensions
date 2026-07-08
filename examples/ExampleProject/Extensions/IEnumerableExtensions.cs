namespace ExampleProject.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> OrderBySequence<T, TId>(
            this IEnumerable<T> source,
            IEnumerable<TId> order,
            Func<T, TId> idSelector
        )
        {
            var lookup = source.ToLookup(idSelector, t => t);
            foreach (var id in order)
            {
                foreach (var t in lookup[id])
                {
                    yield return t;
                }
            }
        }
    }
}
