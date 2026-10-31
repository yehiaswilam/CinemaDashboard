using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;

namespace CinemaDashboard.Services.Interfaces;

public interface ICinemaService
{
    Task<List<Cinema>> GetAllAsync();
    Task<Cinema?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(Cinema model, IFormFile? image);
    Task<ServiceResult> UpdateAsync(Cinema model, IFormFile? image);
    Task<ServiceResult> DeleteAsync(int id);
}
