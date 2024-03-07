using Buildalyzer;
using Microsoft.Build.Construction;

namespace DotNet.Dependencies;

public class Project
{
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    public string[] ProjectReferences { get; set; } = [];
    public string[] PackageReferences { get; set; } = [];
}

public static class DotNetSolution
{
    public static IEnumerable<Project> Analyze(string solutionFile)
    {
        var manager = new AnalyzerManager();
        
        var solution = SolutionFile.Parse(solutionFile);
        foreach (var project in solution.ProjectsInOrder) 
        {
            var analyzer = manager.GetProject(project.AbsolutePath);

            throw new NotImplementedException("add dependencies to returned item");

            yield return new Project()
            {
                Name = project.ProjectName,
                Path = project.AbsolutePath                
            };
        }        
    }    
}
