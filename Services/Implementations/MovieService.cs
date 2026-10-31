using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Files;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class MovieService(
    IUnitOfWork uow,
    ILookupService lookups,
    IImageValidator validator,
    IFileStorage storage) : IMovieService
{
    private const string MainFolder = "movies";
    private const string SubFolder = "movies/sub";

    // ---------------- Queries ----------------

    public Task<List<Movie>> GetAllAsync(MovieFilter f)
    {
        var search = f.Search?.Trim();
        var hasSearch = !string.IsNullOrEmpty(search);
        var categoryId = f.CategoryId is > 0 ? f.CategoryId : null;
        var cinemaId = f.CinemaId is > 0 ? f.CinemaId : null;
        var status = f.Status;

        return uow.Movies.GetAllAsync(
            filter: m => (!hasSearch || m.Name.Contains(search!))
                      && (categoryId == null || m.CategoryId == categoryId)
                      && (cinemaId == null || m.CinemaId == cinemaId)
                      && (status == null || m.Status == status),
            include: q => q.Include(m => m.Category)
                           .Include(m => m.Cinema)
                           .Include(m => m.MovieActors),
            orderBy: q => q.OrderByDescending(m => m.DateTime));
    }

    public Task<Movie?> GetDetailsAsync(int id) =>
        uow.Movies.GetOneAsync(
            m => m.Id == id,
            include: q => q.Include(m => m.Category)
                           .Include(m => m.Cinema)
                           .Include(m => m.SubImages)
                           .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor));

    // ---------------- Forms ----------------

    public async Task<MovieFormVM> GetCreateFormAsync()
    {
        var vm = new MovieFormVM();
        await FillListsAsync(vm);
        return vm;
    }

    public async Task<MovieFormVM?> GetEditFormAsync(int id)
    {
        var movie = await uow.Movies.GetOneAsync(
            m => m.Id == id,
            include: q => q.Include(m => m.SubImages).Include(m => m.MovieActors));

        if (movie is null) return null;

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
        return vm;
    }

    public async Task PopulateFormAsync(MovieFormVM vm)
    {
        if (vm.Id != 0)
        {
            var movie = await uow.Movies.GetOneAsync(m => m.Id == vm.Id, include: q => q.Include(m => m.SubImages));
            vm.ExistingMainImg = movie?.MainImg;
            vm.ExistingSubImages = movie?.SubImages.ToList() ?? new List<MovieImage>();
        }

        await FillListsAsync(vm);
    }

    // ---------------- Commands ----------------

    public async Task<ServiceResult> CreateAsync(MovieFormVM vm)
    {
        var result = await ValidateAsync(vm, mainImageRequired: true);
        if (!result.Succeeded) return result;

        var movie = new Movie();
        MapScalars(vm, movie);
        movie.MainImg = await storage.SaveAsync(vm.MainImgFile!, MainFolder);

        foreach (var file in vm.SubImageFiles.Where(f => f.Length > 0))
            movie.SubImages.Add(new MovieImage { Img = await storage.SaveAsync(file, SubFolder) });

        foreach (var actorId in vm.ActorIds.Distinct())
            movie.MovieActors.Add(new MovieActor { ActorId = actorId });

        await uow.Movies.AddAsync(movie);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Movie \"{movie.Name}\" added.");
    }

    public async Task<ServiceResult> UpdateAsync(MovieFormVM vm)
    {
        var movie = await uow.Movies.GetOneAsync(
            m => m.Id == vm.Id,
            include: q => q.Include(m => m.SubImages).Include(m => m.MovieActors),
            tracked: true);

        if (movie is null) return ServiceResult.NotFoundResult();

        var result = await ValidateAsync(vm, mainImageRequired: false);
        if (!result.Succeeded) return result;

        MapScalars(vm, movie);

        // Main image: replace only if a new one was uploaded
        if (vm.MainImgFile is { Length: > 0 })
        {
            storage.Delete(movie.MainImg);
            movie.MainImg = await storage.SaveAsync(vm.MainImgFile, MainFolder);
        }

        // Sub images: remove the ticked ones, then add the new uploads
        foreach (var img in movie.SubImages.Where(i => vm.RemoveSubImageIds.Contains(i.Id)).ToList())
        {
            storage.Delete(img.Img);
            movie.SubImages.Remove(img);
            uow.MovieImages.Remove(img);
        }

        foreach (var file in vm.SubImageFiles.Where(f => f.Length > 0))
            movie.SubImages.Add(new MovieImage { Img = await storage.SaveAsync(file, SubFolder) });

        SyncActors(movie, vm.ActorIds);

        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Movie \"{movie.Name}\" updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var movie = await uow.Movies.GetOneAsync(
            m => m.Id == id,
            include: q => q.Include(m => m.SubImages),
            tracked: true);

        if (movie is null) return ServiceResult.NotFoundResult();

        storage.Delete(movie.MainImg);
        foreach (var img in movie.SubImages) storage.Delete(img.Img);

        uow.Movies.Remove(movie); // SubImages + MovieActors are removed by cascade
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Movie \"{movie.Name}\" deleted.");
    }

    // ---------------- Helpers ----------------

    private async Task<ServiceResult> ValidateAsync(MovieFormVM vm, bool mainImageRequired)
    {
        var result = ServiceResult.Success();

        var mainError = validator.Validate(vm.MainImgFile, mainImageRequired);
        if (mainError is not null) result.AddError(mainError, nameof(vm.MainImgFile));

        foreach (var file in vm.SubImageFiles)
        {
            var subError = validator.Validate(file, required: false);
            if (subError is not null) result.AddError($"{file.FileName}: {subError}", nameof(vm.SubImageFiles));
        }

        if (!await uow.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            result.AddError("Selected category doesn't exist.", nameof(vm.CategoryId));

        if (!await uow.Cinemas.AnyAsync(c => c.Id == vm.CinemaId))
            result.AddError("Selected cinema doesn't exist.", nameof(vm.CinemaId));

        return result;
    }

    private static void MapScalars(MovieFormVM vm, Movie movie)
    {
        movie.Name = vm.Name;
        movie.Description = vm.Description;
        movie.Price = vm.Price;
        movie.Status = vm.Status;
        movie.DateTime = vm.DateTime;
        movie.CategoryId = vm.CategoryId;
        movie.CinemaId = vm.CinemaId;
    }

    private static void SyncActors(Movie movie, IEnumerable<int> actorIds)
    {
        var selected = actorIds.ToHashSet();

        foreach (var ma in movie.MovieActors.Where(ma => !selected.Contains(ma.ActorId)).ToList())
            movie.MovieActors.Remove(ma);

        var existing = movie.MovieActors.Select(ma => ma.ActorId).ToHashSet();
        foreach (var actorId in selected.Where(a => !existing.Contains(a)))
            movie.MovieActors.Add(new MovieActor { MovieId = movie.Id, ActorId = actorId });
    }

    private async Task FillListsAsync(MovieFormVM vm)
    {
        vm.Categories = await lookups.GetCategoriesAsync();
        vm.Cinemas = await lookups.GetCinemasAsync();
        vm.Actors = await lookups.GetActorsAsync();
    }
}
