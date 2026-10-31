using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Customer.Controllers;

[Area("Customer")]
public class ActorsController(IActorService actors) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var actor = await actors.GetDetailsAsync(id);
        return actor is null ? NotFound() : View(actor);
    }
}
