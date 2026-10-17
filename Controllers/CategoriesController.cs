using CinemaDashboard.Data;
using CinemaDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Controllers;

public class CategoriesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Include(c => c.Movies)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    public IActionResult Create() => View("Form", new Category());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        if (await db.Categories.AnyAsync(c => c.Name == model.Name))
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");

        if (!ModelState.IsValid) return View("Form", model);

        db.Categories.Add(model);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Category \"{model.Name}\" added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await db.Categories.FindAsync(id);
        return category is null ? NotFound() : View("Form", category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        var category = await db.Categories.FindAsync(model.Id);
        if (category is null) return NotFound();

        if (await db.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id))
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");

        if (!ModelState.IsValid) return View("Form", model);

        category.Name = model.Name;
        category.Description = model.Description;
        await db.SaveChangesAsync();

        TempData["Success"] = $"Category \"{category.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await db.Categories.Include(c => c.Movies).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound();

        if (category.Movies.Count > 0)
        {
            TempData["Error"] = $"Can't delete \"{category.Name}\" — it has {category.Movies.Count} movie(s). Move or delete them first.";
            return RedirectToAction(nameof(Index));
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Category \"{category.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
