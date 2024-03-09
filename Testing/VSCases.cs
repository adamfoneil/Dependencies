using Dependency.Abstractions;
using DotNet.Dependencies;

namespace Testing;

[TestClass]
public class VSCases
{
    [TestMethod]
    [DataRow(@"C:\Users\adamo\source\repos\Dependencies\Dependencies.sln", "Dependency.Abstractions;DotNet.Dependencies;Testing")]
    [DataRow(@"C:\Users\adamo\source\repos\Hs5\Hs5.sln", "Hs5.Database;Hs5.Services;Hs5.Test;Hs5.CLI;Hs5.RadzenBlazor;SpayWise;SpayWise.Client")]
    [DataRow(@"C:\Users\adamo\source\repos\Dapper.Entities\Dapper.Entities.sln", "Dapper.Entities;Dapper.Entities.SqlServer;Dapper.Entities.Abstractions;Dapper.Entities.PostgreSql;Testing.Common;Testing.SqlServer;Testing.PostgreSql")]
    public void InspectSolution(string solutionFile, string expected)
    {
        var projects = DotNetSolution.Analyze(solutionFile).ToArray();

        var dependencyOrder = projects.ToValidDependencyOrder(p => p.Name, p => p.ProjectReferences).ToArray();
        var expectedOrder = expected.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.IsTrue(expectedOrder.SequenceEqual(dependencyOrder));
    }
}
