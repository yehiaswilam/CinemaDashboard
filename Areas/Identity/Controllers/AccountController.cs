using CinemaDashboard.Constants;
using CinemaDashboard.Extensions;
using CinemaDashboard.Services.Interfaces;
using CinemaDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Areas.Identity.Controllers;

// /Identity/Account/...  — everything a visitor does before (and while) signing in
[Area("Identity")]
[AllowAnonymous]
public class AccountController(
    IAccountService account,
    IEmailConfirmationService confirmation,
    IPasswordService passwords) : Controller
{
    // =====================================================
    // Login / Logout
    // =====================================================

    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToHome();

        ViewBag.ExternalProviders = await account.GetExternalProvidersAsync();
        return View(new LoginVM { ReturnUrl = returnUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        if (ModelState.IsValid)
        {
            var result = await account.LoginAsync(vm);
            if (result.Succeeded) return await RedirectAfterLoginAsync(vm.Email, vm.ReturnUrl);
            ModelState.AddServiceErrors(result);
        }

        ViewBag.ExternalProviders = await account.GetExternalProvidersAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await account.LogoutAsync();
        return RedirectToAction("Index", "Home", new { area = "Customer" });
    }

    // =====================================================
    // Register + email confirmation
    // =====================================================

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

        return RedirectToAction(nameof(CheckEmail), new { email = vm.Email });
    }

    // "We sent you a link" page (after Register and after Resend)
    public IActionResult CheckEmail(string? email) => View(model: email);

    // Link inside the email: /Identity/Account/ConfirmEmail?userId=...&token=...
    public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
    {
        var result = await confirmation.ConfirmAsync(userId, token);
        return View(result);
    }

    public IActionResult ResendConfirmation(string? email) =>
        View(new ResendConfirmationVM { Email = email ?? string.Empty });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await confirmation.ResendAsync(vm.Email);
        return RedirectToAction(nameof(CheckEmail), new { email = vm.Email });
    }

    // =====================================================
    // Forgot / reset password
    // =====================================================

    public IActionResult ForgotPassword() => View(new ForgotPasswordVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await passwords.ForgotAsync(vm.Email);
        return RedirectToAction(nameof(ForgotPasswordConfirmation), new { email = vm.Email });
    }

    public IActionResult ForgotPasswordConfirmation(string? email) => View(model: email);

    // Link inside the email: /Identity/Account/ResetPassword?email=...&token=...
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "This reset link is invalid. Request a new one.";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View(new ResetPasswordVM { Email = email, Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await passwords.ResetAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddServiceErrors(result);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Login));
    }

    // =====================================================
    // External login (Google)
    // =====================================================

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), new { returnUrl })!;
        var properties = account.ConfigureExternalLogin(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            TempData["Error"] = $"Sign-in was cancelled or failed: {remoteError}";
            return RedirectToAction(nameof(Login));
        }

        var (result, email) = await account.ExternalLoginCallbackAsync();
        if (!result.Succeeded)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Login));
        }

        return await RedirectAfterLoginAsync(email!, returnUrl);
    }

    public IActionResult AccessDenied() => View();

    // =====================================================
    // Helpers
    // =====================================================

    // The User principal isn't refreshed until the next request, so ask the service for the role
    private async Task<IActionResult> RedirectAfterLoginAsync(string email, string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl)) return LocalRedirect(returnUrl!);

        return await account.IsAdminAsync(email)
            ? RedirectToAction("Index", "Home", new { area = "Admin" })
            : RedirectToAction("Index", "Home", new { area = "Customer" });
    }

    private IActionResult RedirectToHome() =>
        User.IsInRole(AppRoles.Admin)
            ? RedirectToAction("Index", "Home", new { area = "Admin" })
            : RedirectToAction("Index", "Home", new { area = "Customer" });
}
