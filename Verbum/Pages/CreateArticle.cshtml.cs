using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Verbum.Services;

namespace Verbum.Pages;

public class CreateArticle(ILogger<CreateArticle> logger, IndexingService indexingService) : PageModel
{
    [BindProperty] public string SubjectName { get; set; } = string.Empty;

    [BindProperty] public string? MarkdownContent { get; set; }

    [BindProperty] public IFormFile? MarkdownFile { get; set; }

    [BindProperty] public IFormFileCollection? Files { get; set; } = new FormFileCollection();

    [BindProperty] public List<string> AdditionalMarkdownFileNames { get; set; } = [];

    [BindProperty] public List<string> AdditionalMarkdownContents { get; set; } = [];

    [BindProperty] public List<IFormFile> AdditionalMarkdownFiles { get; set; } = [];

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

        var formattedSubjectName = ConvertToSlug(SubjectName);
        var subjectFolder = Path.Combine("wwwroot", "articles", formattedSubjectName);

        // Ensure the directory exists
        Directory.CreateDirectory(subjectFolder);

        // Save the root markdown content to a file if provided
        if (!string.IsNullOrEmpty(MarkdownContent))
        {
            var markdownFilePath = Path.Combine(subjectFolder, $"{formattedSubjectName}.md");
            await System.IO.File.WriteAllTextAsync(markdownFilePath, MarkdownContent);
            logger.LogDebug("Saved markdown content to {MarkdownFilePath}", markdownFilePath);

            // Index the root markdown file
            var rootFile = (Name: formattedSubjectName,
                Url: $"/articles/{formattedSubjectName}/{formattedSubjectName}.md");
            await indexingService.IndexFileAsync(rootFile);
        }
        // Save the uploaded root markdown file if provided
        else if (MarkdownFile != null)
        {
            var markdownFilePath = Path.Combine(subjectFolder, $"{formattedSubjectName}.md");
            await using var stream = new FileStream(markdownFilePath, FileMode.Create);
            await MarkdownFile.CopyToAsync(stream);
            logger.LogDebug("Saved uploaded markdown file to {MarkdownFilePath}", markdownFilePath);

            // Index the root markdown file
            var rootFile = (Name: formattedSubjectName,
                Url: $"/articles/{formattedSubjectName}/{formattedSubjectName}.md");
            await indexingService.IndexFileAsync(rootFile);
        }

        // Save the additional markdown contents to files
        for (var i = 0; i < AdditionalMarkdownFileNames.Count; i++)
        {
            var additionalFileName = ConvertToSlug(AdditionalMarkdownFileNames[i]);
            var additionalFilePath = Path.Combine(subjectFolder, $"{additionalFileName}.md");

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

            // Index the additional markdown file
            var additionalFile = (Name: additionalFileName,
                Url: $"/articles/{formattedSubjectName}/{additionalFileName}.md");
            await indexingService.IndexFileAsync(additionalFile);
        }

        // Save the uploaded media files, if any
        foreach (var uploadedFile in Files!)
        {
            var filePath = Path.Combine(subjectFolder, uploadedFile.FileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await uploadedFile.CopyToAsync(stream);
            logger.LogDebug("Saved uploaded file to {FilePath}", filePath);
        }

        return RedirectToPage("/Index");
    }

    private static string ConvertToSlug(string input)
    {
        return input.Replace(" ", "-").ToLowerInvariant();
    }
}