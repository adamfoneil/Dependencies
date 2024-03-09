This started as a friendly challenge at work to determine the "order" of projects in a VS solution according to their project dependencies -- so, projects with no project references would come first. I took a swing at this while at work, but I couldn't get it working. Stuff like that bugs me mercilessly, so wanted to approach the problem with a clean slate. For confidentiality reasons, I couldn't use an open source solution with my company data. But that's okay -- as I wanted to approach the problem generically anyway. Dependency graphs show up in many areas, so I was after a more abstract solution.

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
public static (IReadOnlyList<TKey> Recognized, IReadOnlyList<TKey> Unrecognized) ToDependencyOrder<TItem, TKey>(
  this IEnumerable<TItem> items,
  Func<TItem, TKey> keySelector,
  Func<TItem, IEnumerable<TKey>> childKeySelector) where TKey : notnull
```
Notice it returns two lists of `recognized` and `unrecognized` results. You can ignore the "unrecognized" if you wish with the [ToValidDependencyOrder](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L51) overload. Notice also the use of generics lets you model any sort of dependency graph. I had considered introducing an `INode<T>` interface, but this turned out not to be needed, and would've required calling code to adopt that. (I had envisioned something like [ILookup](https://learn.microsoft.com/en-us/dotnet/api/system.linq.ilookup-2?view=net-8.0).) The pair of delegates -- a key selector `Func<TItem, TKey>` and child dependency selector `Func<TItem, IEnumerable<TKey>>` are all you need to model a dependency graph.

I had some issues with an infinite loop [here](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L23) when, during development, I misspelled one of my ficticious dependencies. This prevented my result list from ever reaching its expected count. It turned out that [excluding the unrecognized](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L27) dependencies kept that from happening.

Another thing needed to prevent infinite looping is detecting circularity, which I do with this method [ThrowIfCircular](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L98).

# Reverse Lookups
There are a couple methods for determining where a dependency is used. I call this a "reverse lookup":
- [ToReverseLookup](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L63) gets just the `TKey`s
- [ToReverseLookupItems](https://github.com/adamfoneil/Dependencies/blob/master/Dependency.Abstractions/Extensions.cs#L57) gets the full `TItem`s

See the [test](https://github.com/adamfoneil/Dependencies/blob/master/Testing/DependencyCases.cs#L67) to get a sense of what this is doing.

# Testing Approach
The initial use case for this application was determining the build order for large Visual Studio solutions. The order mattered for the purpose of targeting some cleanup effort. We needed to work on the lowest-level projects first. In a large solution, it may not be obvious which are the "lowest level" projects -- much less all of them in the right order. That's the reason for this library. I tested this on some of my own solutions [here](https://github.com/adamfoneil/Dependencies/blob/master/Testing/VSCases.cs#L16).

Because the `ToDependencyOrder` method is generic, I had a lot of flexbility in how exactly to prepare the Solution metadata. But I had to get that metadata somehow. That's where the [DotNetSolution](https://github.com/adamfoneil/Dependencies/blob/master/DotNet.Dependencies/DotNetSolution.cs) class came in. I used a library called [Buildalyzer](https://buildalyzer.netlify.app/) to inspect Visual Studio solutions to extract the dependency info I needed from which to build valid unit tests. This, in combination with the [Microsoft.Build](https://github.com/adamfoneil/Dependencies/blob/master/DotNet.Dependencies/DotNet.Dependencies.csproj#L11) package to [parse](https://github.com/adamfoneil/Dependencies/blob/master/DotNet.Dependencies/DotNetSolution.cs#L39) the project names from a .sln file.

I then wrote a little [console app](https://github.com/adamfoneil/Dependencies/blob/master/DotNet.Dependencies.CLI/Program.cs) to scan my local C# solution folder. I ended up picking a handful of solutions that had larger-than-usual dependency graphs. These would provide complex cases for better unit tests. I had a little trouble with solutions that wouldn't analyze properly for one reason or another, so I added a [TryAnalyze](https://github.com/adamfoneil/Dependencies/blob/master/DotNet.Dependencies/DotNetSolution.cs#L22) method that would hide those errors and return only the successes. I saved the output of all this as [Resources](https://github.com/adamfoneil/Dependencies/tree/master/Testing/Resources) for my test project. That way, my tests were not depending on the actual .sln file.

The critical part of the test is [here](https://github.com/adamfoneil/Dependencies/blob/master/Testing/VSCases.cs#L20-L22):

```csharp
var dependencyOrder = projects.ToValidDependencyOrder(
  p => p.Name, 
  p => p.ProjectReferences.Select(path => Path.GetFileNameWithoutExtension(path))).ToArray();
```

Notice I use `Select` on the existing `ProjectReferences`. This is because the `ProjectReferences` contain the full path to the referenced project. I needed to extract just the file name portion so the `ToValidDependencyOrder` method would be matching project `Name` to `Name` instead of a file path, which would never match.
