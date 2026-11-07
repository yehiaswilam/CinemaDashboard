using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

public interface IProfileService
{
    Task<ProfileVM?> GetAsync(string userId);
    Task<ServiceResult> UpdateAsync(string userId, ProfileVM vm);
}
