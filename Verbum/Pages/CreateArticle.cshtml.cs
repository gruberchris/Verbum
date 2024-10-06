using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class CreateArticle(ILogger<CreateArticle> logger) : PageModel
{
    [BindProperty]
    public string ArticleName { get; set; } = string.Empty;

    [BindProperty]
    public string? MarkdownContent { get; set; }

    [BindProperty]
    public IFormFile? MarkdownFile { get; set; }

    [BindProperty]
    public IFormFileCollection? Files { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            logger.LogError("Model state is invalid");

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    logger.LogError("ModelState error in {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                }
            }

            return Page();
        }

        var formattedArticleName = ConvertToSlug(ArticleName);
        var articleDirectory = Path.Combine("wwwroot", "articles", formattedArticleName);

        // Ensure the directory exists
        Directory.CreateDirectory(articleDirectory);

        // Save the markdown content to a file if provided
        if (!string.IsNullOrEmpty(MarkdownContent))
        {
            var markdownFilePath = Path.Combine(articleDirectory, $"{formattedArticleName}.md");
            await System.IO.File.WriteAllTextAsync(markdownFilePath, MarkdownContent);
            logger.LogDebug("Saved markdown content to {MarkdownFilePath}", markdownFilePath);
        }
        // Save the uploaded markdown file if provided
        else if (MarkdownFile != null)
        {
            var markdownFilePath = Path.Combine(articleDirectory, $"{formattedArticleName}.md");
            await using var stream = new FileStream(markdownFilePath, FileMode.Create);
            await MarkdownFile.CopyToAsync(stream);
            logger.LogDebug("Saved uploaded markdown file to {MarkdownFilePath}", markdownFilePath);
        }

        // Save the uploaded media files
        if (Files != null)
        {
            foreach (var file in Files)
            {
                var filePath = Path.Combine(articleDirectory, file.FileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                logger.LogDebug("Saved uploaded file to {FilePath}", filePath);
            }
        }

        return RedirectToPage("/Index");
    }

    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}