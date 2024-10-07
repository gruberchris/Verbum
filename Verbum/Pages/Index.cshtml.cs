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
                var markdownFileName = $"{latestDirectory.Name}.md";
                var markdownFile = latestDirectory.GetFiles(markdownFileName).FirstOrDefault();
                if (markdownFile != null)
                {
                    var markdownContent = await System.IO.File.ReadAllTextAsync(markdownFile.FullName);

                    // Update image URLs to point to the wwwroot folder
                    var articlePath = $"/articles/{latestDirectory.Name}/";
                    markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\((.*?)\)", $"![$1]({articlePath}$2)");

                    // Update markdown links to point to the Article view
                    markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\((.*?\.md)\)", $"[$1](/Article?file=$2&folder={latestDirectory.Name})");

                    LatestArticleContent = Markdown.ToHtml(markdownContent);

                    logger.LogDebug("Rendered the latest article found: {ArticleName}", markdownFile.Name);
                }
            }
        }
    }
}