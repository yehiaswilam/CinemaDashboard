using System.Security.Claims;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Admin.Controllers;

[Area("Admin")]
public class UsersController(IUserService users) : AdminController
{
    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index() => View(await users.GetAllAsync(CurrentUserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(string id) =>
        RedirectWithResult(await users.ToggleAdminAsync(id, CurrentUserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id) =>
        RedirectWithResult(await users.ToggleLockAsync(id, CurrentUserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id) =>
        RedirectWithResult(await users.DeleteAsync(id, CurrentUserId));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmEmail(string id) =>
        RedirectWithResult(await users.ConfirmEmailAsync(id));
}
