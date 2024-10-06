using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Verbum.Pages;

public class CreateArticle(ILogger<SearchResults> logger) : PageModel
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
        
        // Check if the file already exists
        if (System.IO.File.Exists(filePath))
        {
            // Generate a unique ID and append it to the filename
            //var uniqueId = GenerateUniqueId(5);
            //filePath = Path.Combine("Articles", $"{formattedArticleName}-{uniqueId}.md");
            logger.LogWarning("Replacing existing article: {ArticleName}", ArticleName);
            
        }

        // Save the markdown content to a file
        await System.IO.File.WriteAllTextAsync(filePath, MarkdownContent);

        return RedirectToPage("/Index");
    }
    
    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
    
    private static string GenerateUniqueId(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var data = new byte[length];
        RandomNumberGenerator.Fill(data);
        var result = new StringBuilder(length);
        foreach (var byteValue in data)
        {
            result.Append(chars[byteValue % chars.Length]);
        }
        return result.ToString();
    }
}