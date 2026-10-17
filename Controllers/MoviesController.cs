using CinemaDashboard.Data;
using CinemaDashboard.Models;
using CinemaDashboard.Services;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Controllers;

public class MoviesController(ApplicationDbContext db, IFileService files) : Controller
{
    private const string MainFolder = "movies";
    private const string SubFolder = "movies/sub";

    // GET: /Movies?search=&categoryId=&cinemaId=&status=
    public async Task<IActionResult> Index(string? search, int? categoryId, int? cinemaId, MovieStatus? status)
    {
        var query = db.Movies
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Cinema)
            .Include(m => m.MovieActors)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));
        if (categoryId is > 0)
            query = query.Where(m => m.CategoryId == categoryId);
        if (cinemaId is > 0)
            query = query.Where(m => m.CinemaId == cinemaId);
        if (status is not null)
            query = query.Where(m => m.Status == status);

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Categories = new SelectList(await db.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
        ViewBag.Cinemas = new SelectList(await db.Cinemas.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", cinemaId);

        return View(await query.OrderByDescending(m => m.DateTime).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.Category)
            .Include(m => m.Cinema)
            .Include(m => m.SubImages)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(m => m.Id == id);

        return movie is null ? NotFound() : View(movie);
    }

    // ---------------- Create ----------------

    public async Task<IActionResult> Create()
    {
        var vm = new MovieFormVM();
        await FillListsAsync(vm);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieFormVM vm)
    {
        ValidateUploads(vm, mainRequired: true);

        if (!ModelState.IsValid)
        {
            await FillListsAsync(vm);
            return View("Form", vm);
        }

        var movie = new Movie
        {
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            Status = vm.Status,
            DateTime = vm.DateTime,
            CategoryId = vm.CategoryId,
            CinemaId = vm.CinemaId,
            MainImg = await files.SaveAsync(vm.MainImgFile!, MainFolder)
        };

        foreach (var file in vm.SubImageFiles.Where(f => f.Length > 0))
            movie.SubImages.Add(new MovieImage { Img = await files.SaveAsync(file, SubFolder) });

        foreach (var actorId in vm.ActorIds.Distinct())
            movie.MovieActors.Add(new MovieActor { ActorId = actorId });

        db.Movies.Add(movie);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Movie \"{movie.Name}\" added.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- Edit ----------------

    public async Task<IActionResult> Edit(int id)
    {
        var movie = await db.Movies
            .AsNoTracking()
            .Include(m => m.SubImages)
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie is null) return NotFound();

        var vm = new MovieFormVM
        {
            Id = movie.Id,
            Name = movie.Name,
            Description = movie.Description,
            Price = movie.Price,
            Status = movie.Status,
            DateTime = movie.DateTime,
            CategoryId = movie.CategoryId,
            CinemaId = movie.CinemaId,
            ActorIds = movie.MovieActors.Select(ma => ma.ActorId).ToList(),
            ExistingMainImg = movie.MainImg,
            ExistingSubImages = movie.SubImages.ToList()
        };

        await FillListsAsync(vm);
        return View("Form", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MovieFormVM vm)
    {
        var movie = await db.Movies
            .Include(m => m.SubImages)
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == vm.Id);

        if (movie is null) return NotFound();

        ValidateUploads(vm, mainRequired: false);

        if (!ModelState.IsValid)
        {
            vm.ExistingMainImg = movie.MainImg;
            vm.ExistingSubImages = movie.SubImages.ToList();
            await FillListsAsync(vm);
            return View("Form", vm);
        }

        movie.Name = vm.Name;
        movie.Description = vm.Description;
        movie.Price = vm.Price;
        movie.Status = vm.Status;
        movie.DateTime = vm.DateTime;
        movie.CategoryId = vm.CategoryId;
        movie.CinemaId = vm.CinemaId;

        // Main image: replace only if a new one was uploaded
        if (vm.MainImgFile is { Length: > 0 })
        {
            files.Delete(movie.MainImg);
            movie.MainImg = await files.SaveAsync(vm.MainImgFile, MainFolder);
        }

        // Sub images: remove the ticked ones, then add new uploads
        var toRemove = movie.SubImages.Where(i => vm.RemoveSubImageIds.Contains(i.Id)).ToList();
        foreach (var img in toRemove)
        {
            files.Delete(img.Img);
            movie.SubImages.Remove(img);
            db.MovieImages.Remove(img);
        }

        foreach (var file in vm.SubImageFiles.Where(f => f.Length > 0))
            movie.SubImages.Add(new MovieImage { Img = await files.SaveAsync(file, SubFolder) });

        // Actors: sync the join table (remove unticked, add newly ticked)
        var selected = vm.ActorIds.Distinct().ToHashSet();

        foreach (var ma in movie.MovieActors.Where(ma => !selected.Contains(ma.ActorId)).ToList())
            movie.MovieActors.Remove(ma);

        var existing = movie.MovieActors.Select(ma => ma.ActorId).ToHashSet();
        foreach (var actorId in selected.Where(a => !existing.Contains(a)))
            movie.MovieActors.Add(new MovieActor { MovieId = movie.Id, ActorId = actorId });

        await db.SaveChangesAsync();

        TempData["Success"] = $"Movie \"{movie.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- Delete ----------------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await db.Movies.Include(m => m.SubImages).FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null) return NotFound();

        files.Delete(movie.MainImg);
        foreach (var img in movie.SubImages) files.Delete(img.Img);

        db.Movies.Remove(movie); // SubImages + MovieActors are removed by cascade
        await db.SaveChangesAsync();

        TempData["Success"] = $"Movie \"{movie.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ---------------- Helpers ----------------

    private void ValidateUploads(MovieFormVM vm, bool mainRequired)
    {
        var mainError = files.Validate(vm.MainImgFile, mainRequired);
        if (mainError is not null)
            ModelState.AddModelError(nameof(vm.MainImgFile), mainError);

        foreach (var file in vm.SubImageFiles)
        {
            var subError = files.Validate(file, required: false);
            if (subError is not null)
            {
                ModelState.AddModelError(nameof(vm.SubImageFiles), $"{file.FileName}: {subError}");
            }
        }
    }

    private async Task FillListsAsync(MovieFormVM vm)
    {
        vm.Categories = await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        vm.Cinemas = await db.Cinemas
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        vm.Actors = await db.Actors
            .OrderBy(a => a.Name)
            .Select(a => new ActorOption(a.Id, a.Name, a.Img))
            .ToListAsync();
    }
}
