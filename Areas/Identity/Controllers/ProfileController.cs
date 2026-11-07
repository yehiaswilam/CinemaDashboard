using System.Security.Claims;
using CinemaDashboard.Extensions;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Identity.Controllers;

// /Identity/Profile — any signed-in user (Admin or User) manages their own account here
[Area("Identity")]
[Authorize]
public class ProfileController(IProfileService profiles, IPasswordService passwords) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var vm = await profiles.GetAsync(UserId);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileVM vm)
    {
        if (ModelState.IsValid)
        {
            var result = await profiles.UpdateAsync(UserId, vm);
            if (result.NotFound) return NotFound();
            if (result.Succeeded)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddServiceErrors(result);
        }

        // Refill the read-only parts of the page
        var current = await profiles.GetAsync(UserId);
        if (current is null) return NotFound();
        vm.Email = current.Email;
        vm.ProfileImage = current.ProfileImage;
        vm.HasPassword = current.HasPassword;
        vm.ExternalLogins = current.ExternalLogins;
        return View(vm);
    }

    public async Task<IActionResult> ChangePassword() =>
        View(new ChangePasswordVM { HasPassword = await passwords.HasPasswordAsync(UserId) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
    {
        vm.HasPassword = await passwords.HasPasswordAsync(UserId);
        if (!ModelState.IsValid) return View(vm);

        var result = await passwords.ChangeAsync(UserId, vm);
        if (result.NotFound) return NotFound();
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
