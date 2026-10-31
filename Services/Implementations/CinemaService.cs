using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Files;
using CinemaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class CinemaService(IUnitOfWork uow, IImageValidator validator, IFileStorage storage) : ICinemaService
{
    private const string ImgFolder = "cinemas";
    public const string ImageField = "imgFile";

    public Task<List<Cinema>> GetAllAsync() =>
        uow.Cinemas.GetAllAsync(
            include: q => q.Include(c => c.Movies),
            orderBy: q => q.OrderBy(c => c.Name));

    public Task<Cinema?> GetByIdAsync(int id) => uow.Cinemas.GetByIdAsync(id);

    public async Task<ServiceResult> CreateAsync(Cinema model, IFormFile? image)
    {
        var error = validator.Validate(image, required: true);
        if (error is not null) return ServiceResult.Failure(error, ImageField);

        model.Img = await storage.SaveAsync(image!, ImgFolder);
        await uow.Cinemas.AddAsync(model);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Cinema \"{model.Name}\" added.");
    }

    public async Task<ServiceResult> UpdateAsync(Cinema model, IFormFile? image)
    {
        var cinema = await uow.Cinemas.GetByIdAsync(model.Id);
        if (cinema is null) return ServiceResult.NotFoundResult();

        var error = validator.Validate(image, required: false);
        if (error is not null)
        {
            model.Img = cinema.Img; // keep showing the current image on the form
            return ServiceResult.Failure(error, ImageField);
        }

        cinema.Name = model.Name;
        cinema.Address = model.Address;
        cinema.Description = model.Description;

        if (image is { Length: > 0 })
        {
            storage.Delete(cinema.Img);
            cinema.Img = await storage.SaveAsync(image, ImgFolder);
        }

        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Cinema \"{cinema.Name}\" updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var cinema = await uow.Cinemas.GetByIdAsync(id);
        if (cinema is null) return ServiceResult.NotFoundResult();

        var moviesCount = await uow.Movies.CountAsync(m => m.CinemaId == id);
        if (moviesCount > 0)
            return ServiceResult.Failure($"Can't delete \"{cinema.Name}\" — it has {moviesCount} movie(s). Move or delete them first.");

        storage.Delete(cinema.Img);
        uow.Cinemas.Remove(cinema);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Cinema \"{cinema.Name}\" deleted.");
    }
}
