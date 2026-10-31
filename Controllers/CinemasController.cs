using CinemaDashboard.Controllers.Base;
using CinemaDashboard.Extensions;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Controllers;

public class CinemasController(ICinemaService cinemas) : AdminController
{
    public async Task<IActionResult> Index() => View(await cinemas.GetAllAsync());

    public IActionResult Create() => View("Form", new Cinema());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cinema model, IFormFile? imgFile)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var result = await cinemas.CreateAsync(model, imgFile);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cinema = await cinemas.GetByIdAsync(id);
        return cinema is null ? NotFound() : View("Form", cinema);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Cinema model, IFormFile? imgFile)
    {
        if (!ModelState.IsValid)
        {
            model.Img = (await cinemas.GetByIdAsync(model.Id))?.Img;
            return View("Form", model);
        }

        var result = await cinemas.UpdateAsync(model, imgFile);
        if (result.NotFound) return NotFound();
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) => RedirectWithResult(await cinemas.DeleteAsync(id));
}
