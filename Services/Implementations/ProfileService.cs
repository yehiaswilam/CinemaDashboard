using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Files;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Services.Implementations;

public class ProfileService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IImageValidator validator,
    IFileStorage storage) : IProfileService
{
    private const string ImgFolder = "profiles";

    public async Task<ProfileVM?> GetAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var logins = await userManager.GetLoginsAsync(user);

        return new ProfileVM
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email ?? string.Empty,
            ProfileImage = user.ProfileImage,
            HasPassword = await userManager.HasPasswordAsync(user),
            ExternalLogins = logins.Select(l => l.ProviderDisplayName ?? l.LoginProvider).ToList()
        };
    }

    public async Task<ServiceResult> UpdateAsync(string userId, ProfileVM vm)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.NotFoundResult();

        var imageError = validator.Validate(vm.ImageFile, required: false);
        if (imageError is not null) return ServiceResult.Failure(imageError, nameof(vm.ImageFile));

        user.FullName = vm.FullName;
        user.PhoneNumber = vm.PhoneNumber;

        if (vm.ImageFile is { Length: > 0 })
        {
            storage.Delete(user.ProfileImage);
            user.ProfileImage = await storage.SaveAsync(vm.ImageFile, ImgFolder);
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return ServiceResult.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));

        await signInManager.RefreshSignInAsync(user);
        return ServiceResult.Success("Your profile has been updated.");
    }
}
