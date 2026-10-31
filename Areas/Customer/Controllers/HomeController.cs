using System.Diagnostics;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Customer.Controllers;

// "/" → the public site. Open to everyone (signed in or not), read-only.
[Area("Customer")]
public class HomeController(IMovieService movies, ILookupService lookups) : Controller
{
    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        var vm = new CustomerHomeVM
        {
            Search = search,
            CategoryId = categoryId,
            NowShowing = await movies.GetAllAsync(new MovieFilter(search, categoryId, null, MovieStatus.Available)),
            Upcoming = await movies.GetAllAsync(new MovieFilter(search, categoryId, null, MovieStatus.Upcoming)),
            Categories = await lookups.GetCategoriesAsync(categoryId)
        };

        // Upcoming: soonest first
        vm.Upcoming = vm.Upcoming.OrderBy(m => m.DateTime).ToList();
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
