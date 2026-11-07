using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CinemaDashboard.Services.Email;

/// <summary>Builds the absolute links that go inside account emails.</summary>
public interface IAccountLinkBuilder
{
    string ConfirmEmail(string userId, string token);
    string ResetPassword(string email, string token);
}

public class AccountLinkBuilder(LinkGenerator links, IHttpContextAccessor accessor) : IAccountLinkBuilder
{
    public string ConfirmEmail(string userId, string token) =>
        Build("ConfirmEmail", new { area = "Identity", userId, token = TokenCodec.Encode(token) });

    public string ResetPassword(string email, string token) =>
        Build("ResetPassword", new { area = "Identity", email, token = TokenCodec.Encode(token) });

    private string Build(string action, object values) =>
        links.GetUriByAction(accessor.HttpContext!, action, "Account", values)
        ?? throw new InvalidOperationException($"Could not build a link to Account/{action}.");
}

/// <summary>Identity tokens contain characters like '+' and '/', so they're Base64Url-encoded for URLs.</summary>
public static class TokenCodec
{
    public static string Encode(string token) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

    public static string? Decode(string? encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded)) return null;
        try { return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encoded)); }
        catch (FormatException) { return null; }
    }
}
