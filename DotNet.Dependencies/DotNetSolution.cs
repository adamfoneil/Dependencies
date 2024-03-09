using Buildalyzer;
using Microsoft.Build.Construction;

namespace DotNet.Dependencies;

public class Project
{
    public string Name { get; set; } = default!;
    public string Path { get; set; } = default!;
    /// <summary>
    /// this is critical for testing
    /// </summary>
    public string[] ProjectReferences { get; set; } = [];
    /// <summary>
    /// didn't need this for testing, but I was just curious
    /// </summary>
    public string[] PackageReferences { get; set; } = [];
}

public static class DotNetSolution
{
    public static (bool Success, string? Message, IEnumerable<Project> Results) TryAnalyze(string solutionFile)
    {
        try
        {
            var results = Analyze(solutionFile).ToArray();
            return (true, default, results);
        }
        catch (Exception exc)
        {
            return (false, exc.Message, []);
        }
    }

    public static IEnumerable<Project> Analyze(string solutionFile)
    {
        var manager = new AnalyzerManager();
        
        var solution = SolutionFile.Parse(solutionFile);        

        foreach (var project in solution.ProjectsInOrder) 
        {
            var analyzer = manager.GetProject(project.AbsolutePath);
            var info = analyzer.Build().First(); // assumes single target

            yield return new Project()
            {
                Name = project.ProjectName,
                Path = project.AbsolutePath,
                ProjectReferences = info.ProjectReferences.ToArray(),
                PackageReferences = info.PackageReferences.Select(kp => kp.Key).ToArray()
            };
        }
    }    
}
