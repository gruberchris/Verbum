using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class Article : PageModel
{
    private readonly ILogger<Article> _logger;

    public Article(ILogger<Article> logger)
    {
        _logger = logger;
    }

    public string ArticleContent { get; set; }

    public async Task OnGetAsync(string articleName)
    {
        if (string.IsNullOrEmpty(articleName))
        {
            ArticleContent = "Article not found.";
            return;
        }

        var filePath = Path.Combine("wwwroot", "articles", $"{articleName}.md");
        if (System.IO.File.Exists(filePath))
        {
            var markdownContent = await System.IO.File.ReadAllTextAsync(filePath);
            ArticleContent = Markdown.ToHtml(markdownContent);
        }
        else
        {
            ArticleContent = "Article not found.";
        }
    }
}