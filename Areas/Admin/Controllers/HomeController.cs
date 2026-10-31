using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Admin.Controllers;

// /Admin  →  the dashboard home page
[Area("Admin")]
public class HomeController(IDashboardService dashboard) : AdminController
{
    public async Task<IActionResult> Index() => View(await dashboard.GetDashboardAsync());
}
