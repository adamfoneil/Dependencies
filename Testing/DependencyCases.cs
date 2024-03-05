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

        var actual = projects.ToDependencyOrder(p => p.Name, p => p.Dependencies);

        string[] expected =
        [
            "Logger",
            "VariousAbstractions",
            "DomainModels",
            "Services",
            "PublicApi",
            "WebApp"
        ];

        Assert.IsTrue(actual.Valid.SequenceEqual(expected));
        Assert.IsTrue(!actual.Invalid.Any());
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

        var actual = projects.ToDependencyOrder(p => p.Name, p => p.Dependencies);

        string[] expected =
        [
            "Logger",
            "VariousAbstractions",
            "DomainModels",
            "Services",
            "PublicApi",
            "WebApp"
        ];

        Assert.IsTrue(actual.Valid.SequenceEqual(expected));
        Assert.IsTrue(actual.Invalid.SequenceEqual([ "Squiggly" ]));
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
        catch (Exception)
        {

            throw;
        }
        
    }
}