using System.Collections.Concurrent;
using System.Diagnostics;

namespace Verbum.Services;

public class IndexingService(ILogger<IndexingService> logger)
{
    private static readonly ConcurrentDictionary<string, List<(string Name, string Url)>> Index = new();

    public async Task BuildIndexAsync()
    {
        // Start a stopwatch to measure the time taken to index all files
        var stopwatch = Stopwatch.StartNew();

        var articlesDirectory = Path.Combine("wwwroot", "articles");

        if (Directory.Exists(articlesDirectory))
        {
            var directoryInfo = new DirectoryInfo(articlesDirectory);

            var matchedFiles = directoryInfo
                .GetFiles("*.md", SearchOption.AllDirectories)
                .Select(f => 
                {
                    var subjectName = f.Directory?.Name ?? string.Empty;
                    var articleName = Path.GetFileNameWithoutExtension(f.Name);
                    var url = subjectName.Equals(articleName, StringComparison.OrdinalIgnoreCase) 
                        ? $"/Article?subject={subjectName}" 
                        : $"/Article?subject={subjectName}&article={articleName}";
                    return (Name: articleName, Url: url);
                });

            foreach (var file in matchedFiles)
            {
                logger.LogDebug("Indexing file: {FileName}", file.Name);
                await IndexFileAsync(file);
            }
        }
        else
        {
            logger.LogWarning("Articles directory does not exist: {ArticlesDirectory}", articlesDirectory);
        }

        // Stop the stopwatch and log the time taken to index all files
        stopwatch.Stop();
        logger.LogInformation("Indexing completed in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
    }

    public async Task IndexFileAsync((string Name, string Url) file)
    {
        var terms = file.Name.Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var term in terms)
        {
            logger.LogDebug("Indexing term: {Term} for file: {FileName}", term, file.Name);
            
            await Task.Run(() =>
            {
                Index.AddOrUpdate(term.ToLowerInvariant(), [file], (_, list) =>
                {
                    list.Add(file);
                    return list;
                });
            });
        }
    }

    public async Task<List<(string Name, string Url)>> SearchAsync(string query)
    {
        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var uniqueResults = new HashSet<(string Name, string Url)>();

        foreach (var term in searchTerms)
        {
            var lowerTerm = term.ToLowerInvariant();
            var matchingResults = Index
                .Where(kvp => kvp.Key.Contains(lowerTerm))
                .SelectMany(kvp => kvp.Value);

            foreach (var result in matchingResults)
            {
                uniqueResults.Add(result);
            }
        }

        return await Task.FromResult(uniqueResults.ToList());
    }
}