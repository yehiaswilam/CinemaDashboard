using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Email;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Services.Implementations;

public class PasswordService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailService email,
    IAccountLinkBuilder links) : IPasswordService
{
    public async Task ForgotAsync(string emailAddress)
    {
        var user = await userManager.FindByEmailAsync(emailAddress);

        // Only confirmed accounts can reset — and we never say whether the email exists
        if (user is null || !user.EmailConfirmed) return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = links.ResetPassword(user.Email!, token);

        await email.SendAsync(user.Email!, "Reset your password", EmailTemplates.ResetPassword(user.FullName, link));
    }

    public async Task<ServiceResult> ResetAsync(ResetPasswordVM vm)
    {
        var token = TokenCodec.Decode(vm.Token);
        var user = await userManager.FindByEmailAsync(vm.Email);

        // Same message either way, so the form doesn't reveal which emails exist
        if (user is null || token is null)
            return ServiceResult.Failure("This reset link is invalid or has expired.");

        var result = await userManager.ResetPasswordAsync(user, token, vm.Password);
        if (result.Succeeded)
        {
            // A successful reset also clears any lockout from failed attempts
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);
            return ServiceResult.Success("Your password has been reset. You can sign in now.");
        }

        return ToResult(result, "This reset link is invalid or has expired.");
    }

    public async Task<ServiceResult> ChangeAsync(string userId, ChangePasswordVM vm)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.NotFoundResult();

        IdentityResult result;
        if (await userManager.HasPasswordAsync(user))
        {
            if (string.IsNullOrEmpty(vm.CurrentPassword))
                return ServiceResult.Failure("Enter your current password.", nameof(vm.CurrentPassword));

            result = await userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        }
        else
        {
            result = await userManager.AddPasswordAsync(user, vm.NewPassword);
        }

        if (!result.Succeeded) return ToResult(result);

        // Keeps the current session valid after the security stamp changes
        await signInManager.RefreshSignInAsync(user);
        return ServiceResult.Success("Your password has been updated.");
    }

    public async Task<bool> HasPasswordAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await userManager.HasPasswordAsync(user);
    }

    private static ServiceResult ToResult(IdentityResult identity, string? tokenErrorMessage = null)
    {
        var result = new ServiceResult();
        foreach (var e in identity.Errors)
        {
            var message = e.Code == "InvalidToken" && tokenErrorMessage is not null ? tokenErrorMessage : e.Description;
            var key = e.Code == "PasswordMismatch" ? nameof(ChangePasswordVM.CurrentPassword) : "";
            result.AddError(message, key);
        }
        return result;
    }
}
