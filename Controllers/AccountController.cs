using CinemaDashboard.Extensions;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Controllers;

[AllowAnonymous]
public class AccountController(IAccountService account) : Controller
{
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
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

        return Url.IsLocalUrl(vm.ReturnUrl)
            ? LocalRedirect(vm.ReturnUrl!)
            : RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
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
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await account.LogoutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
