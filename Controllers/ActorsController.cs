using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Controllers;

public class ActorsController(ApplicationDbContext db, IFileService files) : Controller
{
    private const string ImgFolder = "actors";

    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Actors.AsNoTracking().Include(a => a.MovieActors).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Name.Contains(search));

        ViewBag.Search = search;
        return View(await query.OrderBy(a => a.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var actor = await db.Actors
            .AsNoTracking()
            .Include(a => a.MovieActors).ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.Id == id);

        return actor is null ? NotFound() : View(actor);
    }

    public IActionResult Create() => View("Form", new Actor());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Actor model, IFormFile? imgFile)
    {
        var imgError = files.Validate(imgFile, required: true);
        if (imgError is not null) ModelState.AddModelError("imgFile", imgError);

        if (!ModelState.IsValid) return View("Form", model);

        model.Img = await files.SaveAsync(imgFile!, ImgFolder);
        db.Actors.Add(model);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Actor \"{model.Name}\" added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var actor = await db.Actors.FindAsync(id);
        return actor is null ? NotFound() : View("Form", actor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Actor model, IFormFile? imgFile)
    {
        var actor = await db.Actors.FindAsync(model.Id);
        if (actor is null) return NotFound();

        var imgError = files.Validate(imgFile, required: false);
        if (imgError is not null) ModelState.AddModelError("imgFile", imgError);

        if (!ModelState.IsValid)
        {
            model.Img = actor.Img;
            return View("Form", model);
        }

        actor.Name = model.Name;
        actor.Bio = model.Bio;

        if (imgFile is not null && imgFile.Length > 0)
        {
            files.Delete(actor.Img);
            actor.Img = await files.SaveAsync(imgFile, ImgFolder);
        }

        await db.SaveChangesAsync();

        TempData["Success"] = $"Actor \"{actor.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await db.Actors.FindAsync(id);
        if (actor is null) return NotFound();

        // MovieActor rows are removed automatically (cascade)
        files.Delete(actor.Img);
        db.Actors.Remove(actor);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Actor \"{actor.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }
}
