using CinemaDashboard.Constants;
using CinemaDashboard.Extensions;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Identity.Controllers;

// /Identity/Account/Login, /Identity/Account/Register ...
[Area("Identity")]
[AllowAnonymous]
public class AccountController(IAccountService account) : Controller
{
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome();
        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await account.LoginAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(vm);
        }

        if (Url.IsLocalUrl(vm.ReturnUrl)) return LocalRedirect(vm.ReturnUrl!);

        // The User principal isn't refreshed until the next request, so ask the service for the role
        return await account.IsAdminAsync(vm.Email)
            ? RedirectToAction("Index", "Home", new { area = "Admin" })
            : RedirectToAction("Index", "Home", new { area = "Customer" });
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome();
        return View(new RegisterVM());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await account.RegisterAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction("Index", "Home", new { area = "Customer" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await account.LogoutAsync();
        return RedirectToAction("Index", "Home", new { area = "Customer" });
    }

    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToHome() =>
        User.IsInRole(AppRoles.Admin)
            ? RedirectToAction("Index", "Home", new { area = "Admin" })
            : RedirectToAction("Index", "Home", new { area = "Customer" });
}
