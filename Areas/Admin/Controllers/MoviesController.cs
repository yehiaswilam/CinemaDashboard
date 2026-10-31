using CinemaDashboard.Extensions;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Admin.Controllers;

[Area("Admin")]
public class MoviesController(IMovieService movies, ILookupService lookups) : AdminController
{
    // GET: /Movies?search=&categoryId=&cinemaId=&status=
    public async Task<IActionResult> Index(string? search, int? categoryId, int? cinemaId, MovieStatus? status)
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Categories = await lookups.GetCategoriesAsync(categoryId);
        ViewBag.Cinemas = await lookups.GetCinemasAsync(cinemaId);

        return View(await movies.GetAllAsync(new MovieFilter(search, categoryId, cinemaId, status)));
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await movies.GetDetailsAsync(id);
        return movie is null ? NotFound() : View(movie);
    }

    public async Task<IActionResult> Create() => View("Form", await movies.GetCreateFormAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieFormVM vm)
    {
        if (ModelState.IsValid)
        {
            var result = await movies.CreateAsync(vm);
            if (result.Succeeded) return RedirectWithResult(result);
            ModelState.AddServiceErrors(result);
        }

        await movies.PopulateFormAsync(vm);
        return View("Form", vm);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var vm = await movies.GetEditFormAsync(id);
        return vm is null ? NotFound() : View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MovieFormVM vm)
    {
        if (ModelState.IsValid)
        {
            var result = await movies.UpdateAsync(vm);
            if (result.NotFound) return NotFound();
            if (result.Succeeded) return RedirectWithResult(result);
            ModelState.AddServiceErrors(result);
        }

        await movies.PopulateFormAsync(vm);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) => RedirectWithResult(await movies.DeleteAsync(id));
}
