using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Files;
using CinemaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class ActorService(IUnitOfWork uow, IImageValidator validator, IFileStorage storage) : IActorService
{
    private const string ImgFolder = "actors";
    public const string ImageField = "imgFile";

    public Task<List<Actor>> GetAllAsync(string? search) =>
        uow.Actors.GetAllAsync(
            filter: string.IsNullOrWhiteSpace(search) ? null : a => a.Name.Contains(search!),
            include: q => q.Include(a => a.MovieActors),
            orderBy: q => q.OrderBy(a => a.Name));

    public Task<Actor?> GetByIdAsync(int id) => uow.Actors.GetByIdAsync(id);

    public Task<Actor?> GetDetailsAsync(int id) =>
        uow.Actors.GetOneAsync(
            a => a.Id == id,
            include: q => q.Include(a => a.MovieActors).ThenInclude(ma => ma.Movie));

    public async Task<ServiceResult> CreateAsync(Actor model, IFormFile? image)
    {
        var error = validator.Validate(image, required: true);
        if (error is not null) return ServiceResult.Failure(error, ImageField);

        model.Img = await storage.SaveAsync(image!, ImgFolder);
        await uow.Actors.AddAsync(model);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Actor \"{model.Name}\" added.");
    }

    public async Task<ServiceResult> UpdateAsync(Actor model, IFormFile? image)
    {
        var actor = await uow.Actors.GetByIdAsync(model.Id);
        if (actor is null) return ServiceResult.NotFoundResult();

        var error = validator.Validate(image, required: false);
        if (error is not null)
        {
            model.Img = actor.Img;
            return ServiceResult.Failure(error, ImageField);
        }

        actor.Name = model.Name;
        actor.Bio = model.Bio;

        if (image is { Length: > 0 })
        {
            storage.Delete(actor.Img);
            actor.Img = await storage.SaveAsync(image, ImgFolder);
        }

        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Actor \"{actor.Name}\" updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var actor = await uow.Actors.GetByIdAsync(id);
        if (actor is null) return ServiceResult.NotFoundResult();

        // MovieActor rows are removed automatically (cascade)
        storage.Delete(actor.Img);
        uow.Actors.Remove(actor);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Actor \"{actor.Name}\" deleted.");
    }
}
