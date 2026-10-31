using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaDashboard.Services.Interfaces;

/// <summary>Dropdown / picker data shared by several screens (SRP: one place builds these lists).</summary>
public interface ILookupService
{
    Task<List<SelectListItem>> GetCategoriesAsync(int? selectedId = null);
    Task<List<SelectListItem>> GetCinemasAsync(int? selectedId = null);
    Task<List<ActorOption>> GetActorsAsync();
}
