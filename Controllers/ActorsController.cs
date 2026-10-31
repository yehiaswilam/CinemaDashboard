using CinemaDashboard.Controllers.Base;
using CinemaDashboard.Extensions;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Controllers;

public class ActorsController(IActorService actors) : AdminController
{
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        return View(await actors.GetAllAsync(search));
    }

    public async Task<IActionResult> Details(int id)
    {
        var actor = await actors.GetDetailsAsync(id);
        return actor is null ? NotFound() : View(actor);
    }

    public IActionResult Create() => View("Form", new Actor());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Actor model, IFormFile? imgFile)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var result = await actors.CreateAsync(model, imgFile);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var actor = await actors.GetByIdAsync(id);
        return actor is null ? NotFound() : View("Form", actor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Actor model, IFormFile? imgFile)
    {
        if (!ModelState.IsValid)
        {
            model.Img = (await actors.GetByIdAsync(model.Id))?.Img;
            return View("Form", model);
        }

        var result = await actors.UpdateAsync(model, imgFile);
        if (result.NotFound) return NotFound();
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) => RedirectWithResult(await actors.DeleteAsync(id));
}
