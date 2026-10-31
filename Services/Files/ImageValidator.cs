using Microsoft.Extensions.Options;

namespace CinemaDashboard.Services.Files;

public class ImageValidator(IOptions<ImageUploadOptions> options) : IImageValidator
{
    private static readonly string[] DefaultExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    private readonly ImageUploadOptions _options = options.Value;
    private string[] Allowed => _options.AllowedExtensions.Length > 0 ? _options.AllowedExtensions : DefaultExtensions;

    public string? Validate(IFormFile? file, bool required)
    {
        if (file is null || file.Length == 0)
            return required ? "Please choose an image." : null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            return $"Only {string.Join(", ", Allowed)} images are allowed.";

        if (file.Length > _options.MaxSizeMB * 1024L * 1024L)
            return $"Image size must be {_options.MaxSizeMB} MB or less.";

        return null;
    }
}
