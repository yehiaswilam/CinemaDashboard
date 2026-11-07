using System.Security.Claims;
using CinemaDashboard.Constants;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Services.Implementations;

public class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailConfirmationService confirmation,
    ILogger<AccountService> logger) : IAccountService
{
    public async Task<ServiceResult> RegisterAsync(RegisterVM vm)
    {
        var user = new ApplicationUser
        {
            FullName = vm.FullName,
            Email = vm.Email,
            UserName = vm.Email
        };

        var created = await userManager.CreateAsync(user, vm.Password);
        if (!created.Succeeded) return ToResult(created);

        // New accounts are normal users; an Admin can promote them from the Users page
        await userManager.AddToRoleAsync(user, AppRoles.User);

        // No sign-in yet: the email has to be confirmed first
        try
        {
            await confirmation.SendAsync(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send the confirmation email to {Email}", user.Email);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> LoginAsync(LoginVM vm)
    {
        var user = await userManager.FindByEmailAsync(vm.Email);
        if (user is null) return ServiceResult.Failure("Invalid email or password.");

        var result = await signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded) return ServiceResult.Success();
        if (result.IsLockedOut) return ServiceResult.Failure("This account is locked. Try again later or contact an admin.");
        if (result.IsNotAllowed) return ServiceResult.Failure("Please confirm your email before signing in.");
        return ServiceResult.Failure("Invalid email or password.");
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();

    public async Task<bool> IsAdminAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.IsInRoleAsync(user, AppRoles.Admin);
    }

    // =====================================================
    // External login (Google)
    // =====================================================

    public async Task<List<ExternalProviderVM>> GetExternalProvidersAsync() =>
        (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Select(s => new ExternalProviderVM(s.Name, s.DisplayName ?? s.Name))
            .ToList();

    public AuthenticationProperties ConfigureExternalLogin(string provider, string redirectUrl) =>
        signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

    public async Task<(ServiceResult Result, string? Email)> ExternalLoginCallbackAsync()
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return (ServiceResult.Failure("Couldn't sign you in with that provider. Please try again."), null);

        // 1) This Google account is already linked to a user → just sign in
        var linkedUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (linkedUser is not null)
            return await SignInExternalAsync(linkedUser, info.LoginProvider);

        // 2) Not linked yet → find the user by email, or create one
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return (ServiceResult.Failure("Your Google account didn't share an email address."), null);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                EmailConfirmed = true // Google already verified this address
            };

            var created = await userManager.CreateAsync(user); // no password — can be set later from the profile
            if (!created.Succeeded) return (ToResult(created), null);

            await userManager.AddToRoleAsync(user, AppRoles.User);
        }

        var linked = await userManager.AddLoginAsync(user, info);
        if (!linked.Succeeded) return (ToResult(linked), null);

        return await SignInExternalAsync(user, info.LoginProvider);
    }

    private async Task<(ServiceResult, string?)> SignInExternalAsync(ApplicationUser user, string provider)
    {
        if (await userManager.IsLockedOutAsync(user))
            return (ServiceResult.Failure("This account is locked. Try again later or contact an admin."), null);

        // Signing in with Google proves the user owns the email
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        await signInManager.SignInAsync(user, isPersistent: false, authenticationMethod: provider);
        return (ServiceResult.Success(), user.Email);
    }

    private static ServiceResult ToResult(IdentityResult identity)
    {
        var result = new ServiceResult();
        foreach (var e in identity.Errors)
            result.AddError(e.Description);
        return result;
    }
}
