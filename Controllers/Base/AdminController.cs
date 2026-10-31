using CinemaDashboard.Constants;
using CinemaDashboard.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaDashboard.Controllers.Base;

/// <summary>
/// Base for every dashboard controller: only Admins can reach them,
/// and they share the same way of showing a service result.
/// </summary>
[Authorize(Roles = AppRoles.Admin)]
public abstract class AdminController : Controller
{
    /// <summary>Flash message + redirect to Index, or 404 when the item doesn't exist.</summary>
    protected IActionResult RedirectWithResult(ServiceResult result)
    {
        if (result.NotFound) return NotFound();

        if (result.Succeeded) TempData["Success"] = result.Message;
        else TempData["Error"] = result.ErrorMessage;

        return RedirectToAction("Index");
    }
}
