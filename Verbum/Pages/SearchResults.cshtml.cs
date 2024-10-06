using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class SearchResults(ILogger<SearchResults> logger) : PageModel
{
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

        var articlesDirectory = Path.Combine("wwwroot", "articles");

        if (Directory.Exists(articlesDirectory))
        {
            var directoryInfo = new DirectoryInfo(articlesDirectory);
            var uniqueResults = new HashSet<(string Name, string Url)>();
            var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            var matchedFiles = directoryInfo
                .GetFiles("*.md", SearchOption.AllDirectories)
                .Where(f => searchTerms.Exists(term => f.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(f => (Name: Path.GetFileNameWithoutExtension(f.Name), Url: f.Directory != null ? $"/articles/{f.Directory.Name}/{f.Name}" : string.Empty));

            var matchedDirectories = directoryInfo
                .GetDirectories("*", SearchOption.AllDirectories)
                .Where(d => searchTerms.Exists(term => d.Name.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .Select(d => (d.Name, Url: $"/articles/{d.Name}"))
                .Where(d => !matchedFiles.Any(f => f.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase)));

            foreach (var file in matchedFiles)
            {
                uniqueResults.Add(file);
            }

            foreach (var directory in matchedDirectories)
            {
                uniqueResults.Add(directory);
            }

            MatchedArticles = uniqueResults.ToArray();

            logger.LogDebug("Matched articles and directories: {MatchedArticles}", MatchedArticles);
        }

        return Task.CompletedTask;
    }

    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}