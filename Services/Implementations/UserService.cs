using CinemaDashboard.Constants;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class UserService(UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<List<UserListItemVM>> GetAllAsync(string currentUserId)
    {
        var users = await userManager.Users.OrderBy(u => u.CreatedAt).ToListAsync();
        var admins = (await userManager.GetUsersInRoleAsync(AppRoles.Admin)).Select(u => u.Id).ToHashSet();

        return users.Select(u => new UserListItemVM
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            IsAdmin = admins.Contains(u.Id),
            IsLocked = u.LockoutEnd is not null && u.LockoutEnd > DateTimeOffset.UtcNow,
            IsCurrentUser = u.Id == currentUserId,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<ServiceResult> ToggleAdminAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId) return ServiceResult.Failure("You can't change your own role.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.NotFoundResult();

        string message;
        if (await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            await userManager.RemoveFromRoleAsync(user, AppRoles.Admin);
            if (!await userManager.IsInRoleAsync(user, AppRoles.User))
                await userManager.AddToRoleAsync(user, AppRoles.User);
            message = $"{user.Email} is no longer an admin.";
        }
        else
        {
            await userManager.AddToRoleAsync(user, AppRoles.Admin);
            message = $"{user.Email} is now an admin.";
        }

        await userManager.UpdateSecurityStampAsync(user); // their next request picks up the new role
        return ServiceResult.Success(message);
    }

    public async Task<ServiceResult> ToggleLockAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId) return ServiceResult.Failure("You can't lock your own account.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.NotFoundResult();

        var isLocked = user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow;

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, isLocked ? null : DateTimeOffset.MaxValue);
        await userManager.UpdateSecurityStampAsync(user); // signs a locked user out of open sessions

        return ServiceResult.Success(isLocked ? $"{user.Email} unlocked." : $"{user.Email} locked.");
    }

    public async Task<ServiceResult> DeleteAsync(string userId, string currentUserId)
    {
        if (userId == currentUserId) return ServiceResult.Failure("You can't delete your own account.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.NotFoundResult();

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? ServiceResult.Success($"{user.Email} deleted.")
            : ServiceResult.Failure(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
