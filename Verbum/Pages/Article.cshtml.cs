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

        var filePath = Path.Combine("Articles", $"{articleName}.md");
        
        logger.LogDebug("Requested article file path: {FilePath}", filePath);
        
        if (System.IO.File.Exists(filePath))
        {
            var markdownContent = await System.IO.File.ReadAllTextAsync(filePath);
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