using CinemaDashboard.Models;
using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CinemaDashboard.Services.Implementations;

public class CategoryService(IUnitOfWork uow) : ICategoryService
{
    public Task<List<Category>> GetAllAsync() =>
        uow.Categories.GetAllAsync(
            include: q => q.Include(c => c.Movies),
            orderBy: q => q.OrderBy(c => c.Name));

    public Task<Category?> GetByIdAsync(int id) => uow.Categories.GetByIdAsync(id);

    public async Task<ServiceResult> CreateAsync(Category model)
    {
        if (await uow.Categories.AnyAsync(c => c.Name == model.Name))
            return ServiceResult.Failure("A category with this name already exists.", nameof(model.Name));

        await uow.Categories.AddAsync(model);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Category \"{model.Name}\" added.");
    }

    public async Task<ServiceResult> UpdateAsync(Category model)
    {
        var category = await uow.Categories.GetByIdAsync(model.Id);
        if (category is null) return ServiceResult.NotFoundResult();

        if (await uow.Categories.AnyAsync(c => c.Name == model.Name && c.Id != model.Id))
            return ServiceResult.Failure("A category with this name already exists.", nameof(model.Name));

        category.Name = model.Name;
        category.Description = model.Description;
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Category \"{category.Name}\" updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var category = await uow.Categories.GetByIdAsync(id);
        if (category is null) return ServiceResult.NotFoundResult();

        var moviesCount = await uow.Movies.CountAsync(m => m.CategoryId == id);
        if (moviesCount > 0)
            return ServiceResult.Failure($"Can't delete \"{category.Name}\" — it has {moviesCount} movie(s). Move or delete them first.");

        uow.Categories.Remove(category);
        await uow.SaveChangesAsync();
        return ServiceResult.Success($"Category \"{category.Name}\" deleted.");
    }
}
