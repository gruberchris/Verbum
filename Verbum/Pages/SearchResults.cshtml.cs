using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class SearchResults : PageModel
{
    private readonly ILogger<SearchResults> _logger;

    public SearchResults(ILogger<SearchResults> logger)
    {
        _logger = logger;
    }

    public string Query { get; set; }
    public (string Name, string Url)[] MatchedArticles { get; set; }

    public async Task OnGetAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            MatchedArticles = Array.Empty<(string, string)>();
            return;
        }
        
        Query = query;

        var articlesDirectory = Path.Combine("wwwroot", "articles");
        if (Directory.Exists(articlesDirectory))
        {
            var files = new DirectoryInfo(articlesDirectory)
                .GetFiles("*.md")
                .Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(f => (Name: Path.GetFileNameWithoutExtension(f.Name), Url: $"/articles/{f.Name}"))
                .ToArray();

            MatchedArticles = files;
        }
    }
}