using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

/// <summary>Admin management of other users (kept apart from IAccountService — ISP).</summary>
public interface IUserService
{
    Task<List<UserListItemVM>> GetAllAsync(string currentUserId);
    Task<ServiceResult> ToggleAdminAsync(string userId, string currentUserId);
    Task<ServiceResult> ToggleLockAsync(string userId, string currentUserId);
    Task<ServiceResult> DeleteAsync(string userId, string currentUserId);
}
