using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class Article(ILogger<Article> logger) : PageModel
{
    public string ArticleContent { get; set; } = string.Empty;

    public async Task OnGetAsync(string articleName)
    {
        if (string.IsNullOrEmpty(articleName))
        {
            ArticleContent = "Article not found.";
            logger.LogWarning("Article name is empty");
            return;
        }

        logger.LogDebug("Article requested: {ArticleName}", articleName);

        var filePath = Path.Combine("wwwroot", "articles", $"{articleName}.md");

        logger.LogDebug("Requested article file path: {FilePath}", filePath);
        
        if (System.IO.File.Exists(filePath))
        {
            var markdownContent = await System.IO.File.ReadAllTextAsync(filePath);

            // Update image and markdown file URLs to point to the wwwroot folder
            var articlePath = $"/articles/{articleName}/";
            markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\((.*?)\)", $"![$1]({articlePath}$2)");
            markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\((.*?\.md)\)", $"[$1]({articlePath}$2)");

            var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
            ArticleContent = Markdown.ToHtml(markdownContent, pipeline);
        }
        else
        {
            ArticleContent = "Article not found.";
            logger.LogWarning("Requested article not found: {ArticleName}", articleName);
        }
    }
}