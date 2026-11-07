using CinemaDashboard.Models;
using CinemaDashboard.Services.Common;
using CinemaDashboard.Services.Email;
using CinemaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CinemaDashboard.Services.Implementations;

public class EmailConfirmationService(
    UserManager<ApplicationUser> userManager,
    IEmailService email,
    IAccountLinkBuilder links) : IEmailConfirmationService
{
    public async Task SendAsync(ApplicationUser user)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = links.ConfirmEmail(user.Id, token);

        await email.SendAsync(user.Email!, "Confirm your email", EmailTemplates.ConfirmEmail(user.FullName, link));
    }

    public async Task<ServiceResult> ConfirmAsync(string? userId, string? encodedToken)
    {
        var token = TokenCodec.Decode(encodedToken);
        if (string.IsNullOrEmpty(userId) || token is null)
            return ServiceResult.Failure("This confirmation link is invalid.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return ServiceResult.Failure("This confirmation link is invalid.");
        if (user.EmailConfirmed) return ServiceResult.Success("Your email is already confirmed. You can sign in.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? ServiceResult.Success("Your email is confirmed. You can sign in now.")
            : ServiceResult.Failure("This confirmation link is invalid or has expired. Request a new one below.");
    }

    public async Task ResendAsync(string emailAddress)
    {
        var user = await userManager.FindByEmailAsync(emailAddress);
        if (user is null || user.EmailConfirmed) return;

        await SendAsync(user);
    }
}
