using CinemaDashboard.Repositories;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaDashboard.Services.Implementations;

public class LookupService(IUnitOfWork uow) : ILookupService
{
    public Task<List<SelectListItem>> GetCategoriesAsync(int? selectedId = null) =>
        uow.Categories.SelectAsync(
            c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == selectedId },
            orderBy: q => q.OrderBy(c => c.Name));

    public Task<List<SelectListItem>> GetCinemasAsync(int? selectedId = null) =>
        uow.Cinemas.SelectAsync(
            c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = c.Id == selectedId },
            orderBy: q => q.OrderBy(c => c.Name));

    public Task<List<ActorOption>> GetActorsAsync() =>
        uow.Actors.SelectAsync(
            a => new ActorOption(a.Id, a.Name, a.Img),
            orderBy: q => q.OrderBy(a => a.Name));
}
