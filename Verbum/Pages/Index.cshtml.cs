using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public string LatestArticleContent { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var articlesDirectory = Path.Combine("Articles");
        
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
                
                logger.LogDebug("Rendered the latest article found: {ArticleName}", latestFile.Name);
            }
        }
    }
}