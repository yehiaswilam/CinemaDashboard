using CinemaDashboard.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaDashboard.ViewModels;

public class CustomerHomeVM
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }

    public List<Movie> NowShowing { get; set; } = [];
    public List<Movie> Upcoming { get; set; } = [];
    public List<SelectListItem> Categories { get; set; } = [];

    public bool IsFiltered => !string.IsNullOrWhiteSpace(Search) || CategoryId is > 0;
}
