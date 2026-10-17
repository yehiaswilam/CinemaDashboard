namespace CinemaDashboard.Services;

public interface IFileService
{
    /// <summary>Returns an error message if the file is not a valid image, otherwise null.</summary>
    string? Validate(IFormFile? file, bool required);

    /// <summary>Saves the image under wwwroot/images/{folder} and returns its relative URL.</summary>
    Task<string> SaveAsync(IFormFile file, string folder);

    void Delete(string? relativePath);
}

public class FileService(IWebHostEnvironment env) : IFileService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    public string? Validate(IFormFile? file, bool required)
    {
        if (file is null || file.Length == 0)
            return required ? "Please choose an image." : null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return "Only .jpg, .jpeg, .png, .gif and .webp images are allowed.";

        if (file.Length > MaxSizeBytes)
            return "Image size must be 5 MB or less.";

        return null;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dir = Path.Combine(env.WebRootPath, "images", folder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/{folder}/{fileName}";
    }

    public void Delete(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var fullPath = Path.Combine(env.WebRootPath,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
}
