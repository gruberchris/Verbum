using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class Article(ILogger<Article> logger) : PageModel
{
    public string ArticleContent { get; set; } = string.Empty;

    public async Task OnGetAsync(string file, string folder)
    {
        if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(folder))
        {
            ArticleContent = "Article not found.";
            logger.LogWarning("File or folder name is empty");
            return;
        }

        logger.LogDebug("Article requested: {File} in folder {Folder}", file, folder);

        var filePath = Path.Combine("wwwroot", "articles", folder, file + ".md");    
        
        if (!System.IO.File.Exists(filePath))
        {
            ArticleContent = "Article not found.";
            logger.LogWarning("Requested article not found: {FilePath}", filePath);
            return;
        }

        var markdownContent = await System.IO.File.ReadAllTextAsync(filePath);
        
        // Update image URLs to point to the wwwroot/articles/{article name}/ folder
        var articlePath = $"/articles/{folder}/";
        markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\((.*?)\)", $"![$1]({articlePath}$2)");
        
        // Update Markdown links to redirect to the Article view
        markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\((.*?)(\.md)\)", $"[$1](/Article?file=$2&folder={folder})");

        var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
        
        ArticleContent = Markdown.ToHtml(markdownContent, pipeline);
    }
}