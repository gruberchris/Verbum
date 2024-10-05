using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class CreateArticle : PageModel
{
    [BindProperty]
    public string ArticleName { get; set; }

    [BindProperty]
    public string MarkdownContent { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var filePath = Path.Combine("wwwroot", "articles", $"{ArticleName}.md");

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // Save the markdown content to a file
        await System.IO.File.WriteAllTextAsync(filePath, MarkdownContent);

        return RedirectToPage("/Index");
    }
}