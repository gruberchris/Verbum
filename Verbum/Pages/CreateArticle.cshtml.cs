using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Verbum.Services;

namespace Verbum.Pages;

public class CreateArticle(ILogger<CreateArticle> logger, IndexingService indexingService) : PageModel
{
    [BindProperty]
    public string ArticleName { get; set; } = string.Empty;

    [BindProperty]
    public string? MarkdownContent { get; set; }

    [BindProperty]
    public IFormFile? MarkdownFile { get; set; }

    [BindProperty]
    public IFormFileCollection? Files { get; set; }

    [BindProperty]
    public List<string> AdditionalMarkdownFileNames { get; set; } = new();

    [BindProperty]
    public List<string> AdditionalMarkdownContents { get; set; } = new();

    [BindProperty]
    public List<IFormFile> AdditionalMarkdownFiles { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            logger.LogDebug("Model state is invalid");

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    logger.LogDebug("ModelState error in {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                }
            }

            return Page();
        }

        var formattedArticleName = ConvertToSlug(ArticleName);
        var articleDirectory = Path.Combine("wwwroot", "articles", formattedArticleName);

        // Ensure the directory exists
        Directory.CreateDirectory(articleDirectory);

        // Save the root markdown content to a file if provided
        if (!string.IsNullOrEmpty(MarkdownContent))
        {
            var markdownFilePath = Path.Combine(articleDirectory, $"{formattedArticleName}.md");
            await System.IO.File.WriteAllTextAsync(markdownFilePath, MarkdownContent);
            logger.LogDebug("Saved markdown content to {MarkdownFilePath}", markdownFilePath);
        }
        // Save the uploaded root markdown file if provided
        else if (MarkdownFile != null)
        {
            var markdownFilePath = Path.Combine(articleDirectory, $"{formattedArticleName}.md");
            await using var stream = new FileStream(markdownFilePath, FileMode.Create);
            await MarkdownFile.CopyToAsync(stream);
            logger.LogDebug("Saved uploaded markdown file to {MarkdownFilePath}", markdownFilePath);
        }

        // Save the additional markdown contents to files
        for (var i = 0; i < AdditionalMarkdownFileNames.Count; i++)
        {
            var additionalFileName = ConvertToSlug(AdditionalMarkdownFileNames[i]);
            var additionalFilePath = Path.Combine(articleDirectory, $"{additionalFileName}.md");

            if (!string.IsNullOrEmpty(AdditionalMarkdownContents[i]))
            {
                await System.IO.File.WriteAllTextAsync(additionalFilePath, AdditionalMarkdownContents[i]);
                logger.LogDebug("Saved additional markdown content to {AdditionalFilePath}", additionalFilePath);
            }
            else if (AdditionalMarkdownFiles.Count > i)
            {
                await using var stream = new FileStream(additionalFilePath, FileMode.Create);
                await AdditionalMarkdownFiles[i].CopyToAsync(stream);
                logger.LogDebug("Saved uploaded additional markdown file to {AdditionalFilePath}", additionalFilePath);
            }
        }

        // Save the uploaded media files
        if (Files != null)
        {
            foreach (var uploadedFile in Files)
            {
                var filePath = Path.Combine(articleDirectory, uploadedFile.FileName);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await uploadedFile.CopyToAsync(stream);
                logger.LogDebug("Saved uploaded file to {FilePath}", filePath);
            }
        }

        // Index the new article
        var file = (Name: formattedArticleName, Url: $"/articles/{formattedArticleName}/{formattedArticleName}.md");
        await indexingService.IndexFileAsync(file);

        return RedirectToPage("/Index");
    }

    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}