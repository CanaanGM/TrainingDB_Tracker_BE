namespace TrainingTests.helpers;

public static class PathHelpers
{
    public static string? GetSolutionRoot()
    {
        string? directory = AppContext.BaseDirectory;
        while (directory != null && !Directory.Exists(Path.Combine(directory, ".git")) && 
           !Directory.GetFiles(directory, "*.sln").Any())
        directory = Directory.GetParent(directory)?.FullName;
        return directory;
    }
}