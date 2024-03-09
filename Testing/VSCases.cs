using Dependency.Abstractions;
using DotNet.Dependencies;
using System.Reflection;
using System.Text.Json;

namespace Testing;

[TestClass]
public class VSCases
{
    [TestMethod]
    [DataRow("Dapper.CX.json", "Dapper.CX.Base;Dapper.CX.SqlServer;Dapper.CX.SqlServer.AspNetCore;SampleApp.Models;SampleApp.Services;Tests.SqlServer;Tests.Base;Tests.ChangeTracking;SampleApp.RazorPages;Tests.CrudService")]
    [DataRow("Hs5.json", "Hs5.Database;Hs5.Services;Hs5.Test;Hs5.CLI;Hs5.RadzenBlazor;SpayWise.Client;SpayWise")]
    [DataRow("Dapper.Entities.json", "Dapper.Entities;Dapper.Entities.SqlServer;Dapper.Entities.Abstractions;Dapper.Entities.PostgreSql;Testing.Common;Testing.SqlServer;Testing.PostgreSql")]
    [DataRow("LiteInvoice3.json", "Radzen.Components;LiteInvoice.Entities;WebApp.Client;LiteInvoice.Server;Tests;LiteInvoice.ServerApp;WebApp")]
    public void InspectSolution(string solutionFile, string expected)
    {        
        var projects = GetProjects(solutionFile);

        var dependencyOrder = projects.ToValidDependencyOrder(
            p => p.Name, 
            p => p.ProjectReferences.Select(path => Path.GetFileNameWithoutExtension(path))).ToArray();

        var expectedOrder = expected.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(expectedOrder.SequenceEqual(dependencyOrder));
    }

    private IEnumerable<Project> GetProjects(string resourceName)
    {
        var json = GetResource(resourceName);
        return JsonSerializer.Deserialize<Project[]>(json) ?? throw new Exception("couldn't deserialize json");
    }

    private static string GetResource(string resourceName)
    {
        var fullPath = $"Testing.Resources.{resourceName}";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullPath) ?? throw new ArgumentException($"Resource not found: {fullPath}");
        return new StreamReader(stream).ReadToEnd();
    }
}
