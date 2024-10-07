using Microsoft.AspNetCore.Mvc.RazorPages;
using Verbum.Services;

namespace Verbum.Pages;

public class SearchResults(ILogger<SearchResults> logger, IndexingService indexingService)
    : PageModel
{
    public string Query { get; set; } = string.Empty;
    public List<(string Name, string Url)> MatchedArticles { get; set; } = [];

    public Task OnGetAsync(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            logger.LogDebug("Search query is empty");
            return Task.CompletedTask;
        }

        Query = query;
        logger.LogDebug("Search query: {Query}", Query);

        MatchedArticles = indexingService.Search(query);

        if (MatchedArticles.Count == 0)
        {
            logger.LogInformation("No articles found for query: {Query}", Query);
        }
        else
        {
            logger.LogDebug("Matched articles and directories: {MatchedArticles}", MatchedArticles);
        }

        return Task.CompletedTask;
    }
}