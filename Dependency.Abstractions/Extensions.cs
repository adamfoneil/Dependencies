namespace Dependency.Abstractions;

public static class Extensions
{
	public static (IReadOnlyList<TKey> Valid, IReadOnlyList<TKey> Invalid) ToDependencyOrder<TItem, TKey>(
		this IEnumerable<TItem> items,
		Func<TItem, TKey> keySelector,
		Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull
	{
		var uniqueItems = items.ToDictionary(keySelector);
		ThrowIfCircular(uniqueItems, childKeySelector);

		var (recognized, unrecognized) = Validate(items, keySelector, childKeySelector);

		HashSet<TKey> results = [];
		
		var rootItems = items.Where(item => !childKeySelector(item).Any())?.ToArray() ?? [];
		var childItems = items.Except(rootItems).ToArray();

		// the root items can be added to output right away
		foreach (var item in rootItems) results.Add(keySelector(item));

		while (results.Count < recognized.Count)
		{
			foreach (var item in childItems)
			{
				var dependencies = childKeySelector(item).Except(unrecognized).ToArray();
				var key = keySelector(item);
				
				// this was just a little debug helper during development
				var missing = dependencies.Except(results).ToArray();

				// if all the dependencies are in the output, we can add this child
				if (dependencies.All(results.Contains))
				{
					if (results.Add(key))
					{
						// if we have everything, we can stop
						if (results.Count == recognized.Count) break;
					}                    
				}
			}
		}

		return ([.. results], unrecognized);
	}

	/// <summary>
	/// returns only the valid/recognized TKeys
	/// </summary>
	public static IReadOnlyList<TKey> ToValidDependencyOrder<TItem, TKey>(
		this IEnumerable<TItem> items,
		Func<TItem, TKey> keySelector,
		Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull =>
		ToDependencyOrder(items, keySelector, childKeySelector).Valid;

	/// <summary>
	/// a dependency graph may have a combination of valid or "recognized" keys,
	/// along with a set of possible invalid or "unrecognized" keys. The unrecognized keys
	/// need to be excluded from any subsequent analysis
	/// </summary>
	public static (IReadOnlyList<TKey> Recognized, IReadOnlyList<TKey> Unrecogized) Validate<TItem, TKey>(
		IEnumerable<TItem> items, 
		Func<TItem, TKey> keySelector,
		Func<TItem, IEnumerable<TKey>> childKeySelector)
	{
		var allReferences = items.SelectMany(childKeySelector).Distinct();
		var allReferenced = items.Select(keySelector).Distinct();

		return 
		(
			allReferenced.Concat(allReferences.Intersect(allReferenced)).Distinct().ToArray(),
			allReferences.Except(allReferenced).ToArray()
		);
	}

	/// <summary>
	/// circular dependencies mess everything up, so you have to catch those before doing anything
	/// </summary>
	private static void ThrowIfCircular<TItem, TKey>(
		Dictionary<TKey, TItem> items,
		Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull
	{
		foreach (var kp in items)
		{
			EnsureNoSelfReference(kp.Key, kp.Value);
		}

		void EnsureNoSelfReference(TKey key, TItem item)
		{
			foreach (var child in childKeySelector(item))
			{
				if (child.Equals(key)) throw new ArgumentException($"Circular reference found: {child}");
				if (items.TryGetValue(child, out var childItem))
				{
                    EnsureNoSelfReference(key, childItem);
                }				
			}
		}
	}
}
