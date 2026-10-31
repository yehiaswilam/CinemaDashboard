using CinemaDashboard.Services.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CinemaDashboard.Extensions;

public static class ModelStateExtensions
{
    /// <summary>Copies a service's errors into ModelState so the form shows them next to the right fields.</summary>
    public static void AddServiceErrors(this ModelStateDictionary modelState, ServiceResult result)
    {
        foreach (var error in result.Errors)
            modelState.AddModelError(error.Key, error.Message);
    }
}
