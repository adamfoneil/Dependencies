using DotNet.Dependencies;
using System.Text.Json;

var path = args.Length > 0 ? args[0] : throw new Exception("Path argument required");
if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"Path not found: {path}");

var outputFolder = Path.Combine(path, "ProjectMetadata.Output");
if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

Console.WriteLine("Getting solution list...");
var solutions = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);

var options = new JsonSerializerOptions() { WriteIndented = true };

foreach (var sln in solutions)
{
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"Analyzing {Path.GetFileName(sln)}");
	
    var (success, message, results) = DotNetSolution.TryAnalyze(sln);
    
    if (success)
    {
        var json = JsonSerializer.Serialize(results, options);
        var outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(sln) + ".json");
        if (File.Exists(outputFile)) File.Delete(outputFile);
        File.WriteAllText(outputFile, json);
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
}