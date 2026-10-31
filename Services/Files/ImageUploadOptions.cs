namespace CinemaDashboard.Services.Files;

/// <summary>
/// Image upload rules, bound from the "ImageUpload" section of appsettings.json.
/// Changing allowed types or size needs no code change (OCP).
/// </summary>
public class ImageUploadOptions
{
    public const string SectionName = "ImageUpload";

    // Left empty on purpose: the config binder appends to arrays, so defaults live in ImageValidator
    public string[] AllowedExtensions { get; set; } = [];
    public int MaxSizeMB { get; set; } = 5;
}
