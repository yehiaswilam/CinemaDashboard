using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

/// <summary>Sign-in / sign-up for the current visitor.</summary>
public interface IAccountService
{
    Task<ServiceResult> RegisterAsync(RegisterVM vm);
    Task<ServiceResult> LoginAsync(LoginVM vm);
    Task LogoutAsync();
    Task<bool> IsAdminAsync(string email);
}
