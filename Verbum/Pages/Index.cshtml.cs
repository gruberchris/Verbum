using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public string LatestArticleContent { get; set; }

    public async Task OnGetAsync()
    {
        var articlesDirectory = Path.Combine("wwwroot", "articles");
        if (Directory.Exists(articlesDirectory))
        {
            var latestFile = new DirectoryInfo(articlesDirectory)
                .GetFiles("*.md")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (latestFile != null)
            {
                var markdownContent = await System.IO.File.ReadAllTextAsync(latestFile.FullName);
                LatestArticleContent = Markdown.ToHtml(markdownContent);
            }
        }
    }
}