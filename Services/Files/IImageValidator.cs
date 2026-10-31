namespace CinemaDashboard.Services.Files;

/// <summary>Only validates images (ISP: separate from saving/deleting files).</summary>
public interface IImageValidator
{
    /// <returns>An error message, or null when the file is acceptable.</returns>
    string? Validate(IFormFile? file, bool required);
}
