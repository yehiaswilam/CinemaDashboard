namespace CinemaDashboard.Services.Common;

/// <summary>
/// Result of a business operation. Services report what went wrong (Key = form field, or "" for general);
/// controllers decide how to show it. This keeps business rules out of the controllers (SRP).
/// </summary>
public class ServiceResult
{
    private readonly List<ServiceError> _errors = [];

    public bool Succeeded => _errors.Count == 0 && !NotFound;
    public bool NotFound { get; private init; }
    public IReadOnlyList<ServiceError> Errors => _errors;
    public string? Message { get; private init; }

    public static ServiceResult Success(string? message = null) => new() { Message = message };

    public static ServiceResult NotFoundResult() => new() { NotFound = true };

    public static ServiceResult Failure(string message, string key = "") =>
        new ServiceResult().AddError(message, key);

    public ServiceResult AddError(string message, string key = "")
    {
        _errors.Add(new ServiceError(key, message));
        return this;
    }

    /// <summary>First error message, handy for TempData alerts.</summary>
    public string ErrorMessage => string.Join(" ", _errors.Select(e => e.Message));
}

public record ServiceError(string Key, string Message);
