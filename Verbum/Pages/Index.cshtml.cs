using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public string LatestArticleContent { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var articlesDirectory = Path.Combine("wwwroot", "articles");

        if (Directory.Exists(articlesDirectory))
        {
            var latestDirectory = new DirectoryInfo(articlesDirectory)
                .GetDirectories()
                .OrderByDescending(d => d.LastWriteTime)
                .FirstOrDefault();

            if (latestDirectory != null)
            {
                var markdownFile = latestDirectory.GetFiles("*.md").FirstOrDefault();
                if (markdownFile != null)
                {
                    var markdownContent = await System.IO.File.ReadAllTextAsync(markdownFile.FullName);

                    // Update image URLs to point to the wwwroot folder
                    var articlePath = $"/articles/{latestDirectory.Name}/";
                    markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\((.*?)\)", $"![$1]({articlePath}$2)");
                    markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\((.*?\.md)\)", $"[$1]({articlePath}$2)");

                    LatestArticleContent = Markdown.ToHtml(markdownContent);

                    logger.LogDebug("Rendered the latest article found: {ArticleName}", markdownFile.Name);
                }
            }
        }
    }
}