using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authentication;

namespace CinemaDashboard.Services.Interfaces;

/// <summary>Sign-up / sign-in / sign-out for the current visitor (local and external).</summary>
public interface IAccountService
{
    Task<ServiceResult> RegisterAsync(RegisterVM vm);
    Task<ServiceResult> LoginAsync(LoginVM vm);
    Task LogoutAsync();
    Task<bool> IsAdminAsync(string email);

    // ---------- External login (Google) ----------
    Task<List<ExternalProviderVM>> GetExternalProvidersAsync();
    AuthenticationProperties ConfigureExternalLogin(string provider, string redirectUrl);

    /// <returns>The signed-in email on success.</returns>
    Task<(ServiceResult Result, string? Email)> ExternalLoginCallbackAsync();
}
