using Dependency.Abstractions;
using DotNet.Dependencies;

namespace Testing;

[TestClass]
public class VSCases
{
    [TestMethod]
    [DataRow(@"C:\Users\adamo\source\repos\Dependencies\Dependencies.sln")]
    [DataRow(@"C:\Users\adamo\source\repos\Hs5\Hs5.sln")]
    public void InspectSolution(string solutionFile)
    {
        var projects = DotNetSolution.Analyze(solutionFile).ToArray();

        var dependencyOrder = projects.ToValidDependencyOrder(p => p.Name, p => p.ProjectReferences);

    }
}
