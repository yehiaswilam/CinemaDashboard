using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;

namespace CinemaDashboard.Services.Interfaces;

public interface IEmailConfirmationService
{
    Task SendAsync(ApplicationUser user);
    Task<ServiceResult> ConfirmAsync(string? userId, string? encodedToken);

    /// <summary>Always "succeeds" so the page never reveals whether an email is registered.</summary>
    Task ResendAsync(string email);
}
