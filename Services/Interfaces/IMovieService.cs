using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

public interface IMovieService
{
    Task<List<Movie>> GetAllAsync(MovieFilter filter);
    Task<Movie?> GetDetailsAsync(int id);

    Task<MovieFormVM> GetCreateFormAsync();
    Task<MovieFormVM?> GetEditFormAsync(int id);

    /// <summary>Refills dropdowns / existing images after a failed post.</summary>
    Task PopulateFormAsync(MovieFormVM vm);

    Task<ServiceResult> CreateAsync(MovieFormVM vm);
    Task<ServiceResult> UpdateAsync(MovieFormVM vm);
    Task<ServiceResult> DeleteAsync(int id);
}
