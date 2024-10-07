using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class SearchResults(ILogger<SearchResults> logger) : PageModel
{
    private static readonly ConcurrentDictionary<string, List<(string Name, string Url)>> Index = new();

    public string Query { get; set; } = string.Empty;
    public (string Name, string Url)[]? MatchedArticles { get; set; }

    public Task OnGet(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            logger.LogWarning("Search query is empty");
            return Task.CompletedTask;
        }

        Query = ConvertToSlug(query);

        logger.LogDebug("Search query: {Query}", Query);

        if (Index.IsEmpty)
        {
            var stopwatch = Stopwatch.StartNew();
            BuildIndex();
            stopwatch.Stop();
            logger.LogInformation("Indexing completed in {ElapsedMilliseconds} ms", stopwatch.ElapsedMilliseconds);
        }

        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var uniqueResults = new HashSet<(string Name, string Url)>();

        foreach (var term in searchTerms)
        {
            if (!Index.TryGetValue(term, out var results))
            {
                logger.LogDebug("No results found for term: {Term}", term);
                continue;
            }

            foreach (var result in results)
            {
                uniqueResults.Add(result);
            }
        }

        MatchedArticles = uniqueResults.ToArray();

        if (MatchedArticles.Length == 0)
        {
            logger.LogInformation("No articles found for query: {Query}", Query);
        }
        else
        {
            logger.LogDebug("Matched articles and directories: {MatchedArticles}", MatchedArticles);
        }

        return Task.CompletedTask;
    }

    private void BuildIndex()
    {
        var articlesDirectory = Path.Combine("wwwroot", "articles");

        if (Directory.Exists(articlesDirectory))
        {
            var directoryInfo = new DirectoryInfo(articlesDirectory);

            var matchedFiles = directoryInfo
                .GetFiles("*.md", SearchOption.AllDirectories)
                .Select(f => (Name: Path.GetFileNameWithoutExtension(f.Name),
                    Url: f.Directory != null ? $"/articles/{f.Directory.Name}/{f.Name}" : string.Empty));

            foreach (var file in matchedFiles)
            {
                logger.LogDebug("Indexing file: {FileName}", file.Name);
                IndexFile(file);
            }
        }
        else
        {
            logger.LogWarning("Articles directory does not exist: {ArticlesDirectory}", articlesDirectory);
        }
    }

    private void IndexFile((string Name, string Url) file)
    {
        var terms = file.Name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var term in terms)
        {
            logger.LogDebug("Indexing term: {Term} for file: {FileName}", term, file.Name);
            Index.AddOrUpdate(term.ToLowerInvariant(), [file], (_, list) =>
            {
                list.Add(file);
                return list;
            });
        }
    }

    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}