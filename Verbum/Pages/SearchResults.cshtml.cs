using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class SearchResults(ILogger<SearchResults> logger) : PageModel
{
    public string Query { get; set; } = string.Empty;
    public (string Name, string Url)[]? MatchedArticles { get; set; }

    public Task OnGet(string query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            Query = ConvertToSlug(query);
        
            logger.LogDebug("Search query: {Query}", Query);

            var articlesDirectory = Path.Combine("Articles");
        
            if (Directory.Exists(articlesDirectory))
            {
                var files = new DirectoryInfo(articlesDirectory)
                    .GetFiles("*.md")
                    .Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(f => (Name: Path.GetFileNameWithoutExtension(f.Name), Url: $"/articles/{f.Name}"))
                    .ToArray();

                MatchedArticles = files;
            
                logger.LogDebug("Matched articles: {MatchedArticles}", files);
            }
        }
        
        return Task.CompletedTask;
    }
    
    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}