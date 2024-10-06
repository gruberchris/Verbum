using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class CreateArticle : PageModel
{
    [BindProperty]
    public string ArticleName { get; set; } = string.Empty;

    [BindProperty]
    public string MarkdownContent { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var formattedArticleName = ConvertToSlug(ArticleName);
        var filePath = Path.Combine("Articles", $"{formattedArticleName}.md");

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        // Save the markdown content to a file
        await System.IO.File.WriteAllTextAsync(filePath, MarkdownContent);

        return RedirectToPage("/Index");
    }
    
    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}