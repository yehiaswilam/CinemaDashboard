using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Controllers;

public class CinemasController(ApplicationDbContext db, IFileService files) : Controller
{
    private const string ImgFolder = "cinemas";

    public async Task<IActionResult> Index()
    {
        var cinemas = await db.Cinemas
            .AsNoTracking()
            .Include(c => c.Movies)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(cinemas);
    }

    public IActionResult Create() => View("Form", new Cinema());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cinema model, IFormFile? imgFile)
    {
        var imgError = files.Validate(imgFile, required: true);
        if (imgError is not null) ModelState.AddModelError("imgFile", imgError);

        if (!ModelState.IsValid) return View("Form", model);

        model.Img = await files.SaveAsync(imgFile!, ImgFolder);
        db.Cinemas.Add(model);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Cinema \"{model.Name}\" added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var cinema = await db.Cinemas.FindAsync(id);
        return cinema is null ? NotFound() : View("Form", cinema);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Cinema model, IFormFile? imgFile)
    {
        var cinema = await db.Cinemas.FindAsync(model.Id);
        if (cinema is null) return NotFound();

        var imgError = files.Validate(imgFile, required: false);
        if (imgError is not null) ModelState.AddModelError("imgFile", imgError);

        if (!ModelState.IsValid)
        {
            model.Img = cinema.Img; // keep showing the current image
            return View("Form", model);
        }

        cinema.Name = model.Name;
        cinema.Address = model.Address;
        cinema.Description = model.Description;

        if (imgFile is not null && imgFile.Length > 0)
        {
            files.Delete(cinema.Img);
            cinema.Img = await files.SaveAsync(imgFile, ImgFolder);
        }

        await db.SaveChangesAsync();

        TempData["Success"] = $"Cinema \"{cinema.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var cinema = await db.Cinemas.Include(c => c.Movies).FirstOrDefaultAsync(c => c.Id == id);
        if (cinema is null) return NotFound();

        if (cinema.Movies.Count > 0)
        {
            TempData["Error"] = $"Can't delete \"{cinema.Name}\" — it has {cinema.Movies.Count} movie(s). Move or delete them first.";
            return RedirectToAction(nameof(Index));
        }

        files.Delete(cinema.Img);
        db.Cinemas.Remove(cinema);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Cinema \"{cinema.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
