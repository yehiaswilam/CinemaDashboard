using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

public interface IPasswordService
{
    /// <summary>Sends a reset link if the account exists. Never reveals whether it does.</summary>
    Task ForgotAsync(string email);

    Task<ServiceResult> ResetAsync(ResetPasswordVM vm);

    /// <summary>Changes the password, or sets one for accounts created with Google.</summary>
    Task<ServiceResult> ChangeAsync(string userId, ChangePasswordVM vm);

    Task<bool> HasPasswordAsync(string userId);
}
