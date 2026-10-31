using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Customer.Controllers;

// Read-only movie page for visitors. Add / Edit / Delete only exist in the Admin area.
[Area("Customer")]
public class MoviesController(IMovieService movies) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var movie = await movies.GetDetailsAsync(id);

        // Expired movies aren't part of the public site
        if (movie is null || movie.Status == MovieStatus.Expired) return NotFound();

        return View(movie);
    }
}
