using System.Text.RegularExpressions;
using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class Article(ILogger<Article> logger) : PageModel
{
    public string ArticleContent { get; set; } = string.Empty;

    public async Task OnGetAsync(string subject, string? article)
    {
        if (string.IsNullOrEmpty(subject))
        {
            ArticleContent = "Article not found: no subject was specified.";
            logger.LogWarning("Subject was not specified");
            return;
        }

        if (string.IsNullOrEmpty(article))
        {
            logger.LogDebug("Subject {Subject} and root article requested", subject);
        }
        else
        {
            logger.LogDebug("Subject {Subject} and article {Article} requested", subject, article);
        }
        
        var markdownFileName = string.IsNullOrEmpty(article) ? $"{subject}.md" : $"{article}.md";
        var filePath = Path.Combine("wwwroot", "articles", subject, markdownFileName);

        if (!System.IO.File.Exists(filePath))
        {
            ArticleContent = "Article not found: file does not exist.";
            logger.LogWarning("Requested markdown file path not found: {FilePath}", filePath);
            return;
        }

        var markdownContent = await System.IO.File.ReadAllTextAsync(filePath);

        // Update image URLs to point to the wwwroot/articles/{subject}/ folder
        var subjectPath = $"/articles/{subject}/";
        markdownContent = Regex.Replace(markdownContent, @"!\[(.*?)\]\((.*?)\)", $"![$1]({subjectPath}$2)");

        // Update Markdown links to redirect to the Article view
        markdownContent = Regex.Replace(markdownContent, @"\[(.*?)\]\((.*?)(\.md)\)", $"[$1](/Article?subject={subject}&article=$2)");

        var pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();

        ArticleContent = Markdown.ToHtml(markdownContent, pipeline);
    }
}