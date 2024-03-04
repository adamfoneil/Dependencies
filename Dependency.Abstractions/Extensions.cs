namespace Dependency.Abstractions;

public static class Extensions
{
    public static IReadOnlyList<TKey> ToDependencyOrder<TItem, TKey>(
        this IEnumerable<TItem> items,
        Func<TItem, TKey> keySelector,
        Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull
    {
        ThrowIfCircular(items);
        ThrowAnyUnrecognized(items);

        HashSet<TKey> results = [];

        var count = items.Count();
        var rootItems = items.Where(item => !childKeySelector(item).Any())?.ToArray() ?? [];
        var childItems = items.Except(rootItems).ToArray();

        // the root items can be added to output right away
        foreach (var item in rootItems) results.Add(keySelector(item));

        while (results.Count < count)
        {
            foreach (var item in childItems)
            {
                var dependencies = childKeySelector(item).ToArray();
                var key = keySelector(item);
                // if all the dependencies are in the output, we can add this child

                var missing = dependencies.Except(results).ToArray();

                if (dependencies.All(results.Contains) && !results.Contains(key))
                {
                    results.Add(key);
                }
            }
        }

        return [.. results];
    }

    private static void ThrowAnyUnrecognized<TItem>(IEnumerable<TItem> items)
    {
        // todo: this is for when I misspelled "DomainModels "
    }

    private static void ThrowIfCircular<TItem>(IEnumerable<TItem> items)
    {
        // todo: not done yet
    }
}
