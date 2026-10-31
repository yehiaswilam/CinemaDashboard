namespace CinemaDashboard.Services.Files;

/// <summary>
/// Where uploaded files live. Today: wwwroot on disk (LocalFileStorage).
/// Tomorrow it could be Azure Blob / S3 — just add a new implementation and swap it in Program.cs
/// without touching any service or controller (OCP + LSP + DIP).
/// </summary>
public interface IFileStorage
{
    /// <returns>The public relative URL of the saved file, e.g. /images/movies/abc.jpg</returns>
    Task<string> SaveAsync(IFormFile file, string folder);

    void Delete(string? relativePath);
}
