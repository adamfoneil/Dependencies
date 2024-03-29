using Dependency.Abstractions;

namespace Testing;

[TestClass]
public class DependencyCases
{
    [TestMethod]
    public void SimpleCase()
    {
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
        Assert.IsTrue(!invalid.Any());
    }

    [TestMethod]
    public void WithInvalidKey()
    {
        var projects = new[]
        {
            new { Name = "WebApp", Dependencies = new[] { "Logger", "Services", "Squiggly" }},
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
        Assert.IsTrue(invalid.SequenceEqual(["Squiggly"]));
    }

    [TestMethod]
    public void ReverseLookup()
    {
        var projects = new[]
        {
            new { Name = "WebApp", Dependencies = new[] { "Logger", "Services", "DomainModels" }},
            new { Name = "Services", Dependencies = new[] { "Logger", "DomainModels" }},
            new { Name = "DomainModels", Dependencies = new[] { "VariousAbstractions" }},
            new { Name = "PublicApi", Dependencies = new[] { "Services" }},
            new { Name = "Logger", Dependencies = Array.Empty<string>() },
            new { Name = "VariousAbstractions", Dependencies = Array.Empty<string>() }
        };

        var lookup = projects.ToReverseLookup(p => p.Name, p => p.Dependencies);

        Assert.IsTrue(lookup["Logger"].SequenceEqual([ "WebApp", "Services" ]));
        Assert.IsTrue(lookup["VariousAbstractions"].SequenceEqual([ "DomainModels" ]));
        Assert.IsTrue(lookup["Services"].SequenceEqual(["WebApp", "PublicApi"]));
    }

    [TestMethod]
    public void CircularCase()
    {
        var items = new[]
        {
            new { Name = "A", Children = new[] { "B", "C", "D" } },
            new { Name = "D", Children = new[] { "E", "F", "A" } },
        };

        try
        {
            var order = items.ToDependencyOrder(item => item.Name, item => item.Children);
        }
        catch (ArgumentException exc)
        {
            Assert.IsTrue(exc.Message.Equals("Circular reference found: A"));
            return;
        }

        Assert.Fail("shouldn't reach this point");
    }
}