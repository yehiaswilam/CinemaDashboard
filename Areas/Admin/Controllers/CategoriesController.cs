using CinemaDashboard.Extensions;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Admin.Controllers;

// Thin controller: HTTP in / view out. All rules live in ICategoryService (SRP + DIP).
[Area("Admin")]
public class CategoriesController(ICategoryService categories) : AdminController
{
    public async Task<IActionResult> Index() => View(await categories.GetAllAsync());

    public IActionResult Create() => View("Form", new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var result = await categories.CreateAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await categories.GetByIdAsync(id);
        return category is null ? NotFound() : View("Form", category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        if (!ModelState.IsValid) return View("Form", model);

        var result = await categories.UpdateAsync(model);
        if (result.NotFound) return NotFound();
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View("Form", model);
        }

        return RedirectWithResult(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) => RedirectWithResult(await categories.DeleteAsync(id));
}
