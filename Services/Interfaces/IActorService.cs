using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;

namespace CinemaDashboard.Services.Interfaces;

public interface IActorService
{
    Task<List<Actor>> GetAllAsync(string? search);
    Task<Actor?> GetByIdAsync(int id);
    Task<Actor?> GetDetailsAsync(int id);
    Task<ServiceResult> CreateAsync(Actor model, IFormFile? image);
    Task<ServiceResult> UpdateAsync(Actor model, IFormFile? image);
    Task<ServiceResult> DeleteAsync(int id);
}
