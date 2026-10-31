using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;

namespace CinemaDashboard.Services.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(Category model);
    Task<ServiceResult> UpdateAsync(Category model);
    Task<ServiceResult> DeleteAsync(int id);
}
