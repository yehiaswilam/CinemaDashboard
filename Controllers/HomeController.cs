using System.Diagnostics;
using CinemaDashboard.Controllers.Base;
using CinemaDashboard.Models;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Controllers;

public class HomeController(IDashboardService dashboard) : AdminController
{
    public async Task<IActionResult> Index() => View(await dashboard.GetDashboardAsync());

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
