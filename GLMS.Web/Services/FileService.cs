namespace GLMS.Web.Services;

public interface IFileService
{
    /// <summary>Saves a PDF file and returns the relative path. Throws if not PDF.</summary>
    Task<string> SavePdfAsync(IFormFile file, string uploadsFolder);
    bool IsPdf(IFormFile file);
}

public class FileService : IFileService
{
    public bool IsPdf(IFormFile file) =>
        file != null &&
        Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase) &&
        file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

    public async Task<string> SavePdfAsync(IFormFile file, string uploadsFolder)
    {
        if (!IsPdf(file))
            throw new InvalidOperationException("Only PDF files are allowed.");

        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine("uploads", fileName); // relative web path
    }
}
