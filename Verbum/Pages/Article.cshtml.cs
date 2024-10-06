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

        var articlesDirectory = Path.Combine("wwwroot", "articles");
        var articleFilePath = Directory
            .GetFiles(articlesDirectory, $"{articleName}.md", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (articleFilePath != null)
        {
            logger.LogDebug("Requested article file path: {FilePath}", articleFilePath);

            var markdownContent = await System.IO.File.ReadAllTextAsync(articleFilePath);
            var articlePath = $"/articles/{Path.GetDirectoryName(articleFilePath)?.Replace(articlesDirectory, "").Trim(Path.DirectorySeparatorChar)}/";
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