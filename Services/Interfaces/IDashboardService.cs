using CinemaDashboard.ViewModels;

namespace CinemaDashboard.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardVM> GetDashboardAsync();
}
