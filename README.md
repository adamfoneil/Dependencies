This started as a friendly challenge at work to determine the "order" of projects in a VS solution according to their project dependencies -- so, projects with no project references would come first. I took a swing at this while at work, but I couldn't get it working. Stuff like that bugs me mercilessly, so wanted to approach the problem with a clean slate. For confidentiality reasons, I couldn't use an open source solution with my company data. But that's okay -- as I wanted to approach the problem generically. Dependency graphs show up in many areas, so I was after a more abstract solution.

The heart of this is the [ToDependencyOrder](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L5) method. See this in action in a simple [test case](https://github.com/adamfoneil/Dependencies/blob/master/Testing/DependencyCases.cs#L9).

```csharp
 var projects = new[]
{
    new { Name = "WebApp", Dependencies = new[] { "Logger", "Services", "DomainModels" }},
    new { Name = "Services", Dependencies = new[] { "Logger", "DomainModels" }},
    new { Name = "DomainModels", Dependencies = new[] { "VariousAbstractions" }},
    new { Name = "PublicApi", Dependencies = new[] { "Services" }},
    new { Name = "Logger", Dependencies = Array.Empty<string>() },
    new { Name = "VariousAbstractions", Dependencies = Array.Empty<string>() }
};

var (valid, invalid) = projects.ToDependencyOrder(p => p.Name, p => p.Dependencies);

string[] expected =
[
    "Logger",
    "VariousAbstractions",
    "DomainModels",
    "Services",
    "PublicApi",
    "WebApp"
];

Assert.IsTrue(valid.SequenceEqual(expected));
```
You can see that "Logger" and "VariousAbstractions" come first because they have no dependencies. "DomainModels" comes next because it has one dependency "VariousAbstractions" that is already part of the output. "WebApp" and "PublicApi" come last because they have dependencies that need to come first.

Here's the `ToDepdencyOrder` method signature:

```csharp
public static (IReadOnlyList<TKey> Valid, IReadOnlyList<TKey> Invalid) ToDependencyOrder<TItem, TKey>(
  this IEnumerable<TItem> items,
  Func<TItem, TKey> keySelector,
  Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull
```
Notice it returns two lists of `valid` and `invalid` results. I'm not sure those are the best terms to use, but "invalid" in this context means a dependency not in the `items` list. You can ignore those if you wish with the [ToValidDependencyOrder](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L51) overload. Notice also the use of generics lets you model any sort of dependency graph. I had considered introducing an `INode<T>` interface, but this turned out not to be needed, and would've required calling code to adopt that. (I had envisioned something like [ILookup](https://learn.microsoft.com/en-us/dotnet/api/system.linq.ilookup-2?view=net-8.0).) The pair of delegates -- a key selector `Func<TItem, TKey>` and child dependency selector `Func<TItem, IEnumerable<TKey>>` are all you need to model a dependency graph.

I had some issues with an infinite loop [here](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L23) when, during development, I misspelled one of my ficticious dependencies. This prevented my result list from ever reaching its expected count. It turned out that [excluding the unrecognized](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L27) dependencies kept that from happening.

Another thing needed to prevent infinite looping is detecting circularity, which I do with this method [ThrowIfCircular](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L80).
