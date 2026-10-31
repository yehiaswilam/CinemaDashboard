using CinemaDashboard.Constants;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Services.Implementations;

public class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAccountService
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
        await signInManager.SignInAsync(user, isPersistent: false);

        return ServiceResult.Success($"Welcome, {user.FullName}!");
    }

    public async Task<ServiceResult> LoginAsync(LoginVM vm)
    {
        var user = await userManager.FindByEmailAsync(vm.Email);
        if (user is null) return ServiceResult.Failure("Invalid email or password.");

        var result = await signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded) return ServiceResult.Success();
        if (result.IsLockedOut) return ServiceResult.Failure("This account is locked. Try again later or contact an admin.");
        return ServiceResult.Failure("Invalid email or password.");
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();

    public async Task<bool> IsAdminAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is not null && await userManager.IsInRoleAsync(user, AppRoles.Admin);
    }

    private static ServiceResult ToResult(IdentityResult identity)
    {
        var result = new ServiceResult();
        foreach (var e in identity.Errors)
            result.AddError(e.Description);
        return result;
    }
}
