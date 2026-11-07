using System.Net;

namespace CinemaDashboard.Services.Email;

/// <summary>HTML bodies for the account emails (SRP: wording/markup lives here, not in the services).</summary>
public static class EmailTemplates
{
    public static string ConfirmEmail(string name, string link) => Layout(
        title: "Confirm your email",
        greeting: name,
        text: "Thanks for creating an account. Please confirm your email address to start using it.",
        buttonText: "Confirm email",
        link: link,
        footer: "If you didn't create this account, you can ignore this email.");

    public static string ResetPassword(string name, string link) => Layout(
        title: "Reset your password",
        greeting: name,
        text: "We got a request to reset your password. The link works for a limited time.",
        buttonText: "Choose a new password",
        link: link,
        footer: "If you didn't ask for this, you can ignore this email — your password won't change.");

    private static string Layout(string title, string greeting, string text, string buttonText, string link, string footer)
    {
        var safeLink = WebUtility.HtmlEncode(link);
        return $"""
            <div style="font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:auto;padding:24px;color:#1f2330">
              <h2 style="margin:0 0 16px">{WebUtility.HtmlEncode(title)}</h2>
              <p>Hi {WebUtility.HtmlEncode(greeting)},</p>
              <p>{text}</p>
              <p style="margin:28px 0">
                <a href="{safeLink}" style="background:#e5484d;color:#fff;padding:12px 22px;border-radius:8px;text-decoration:none;font-weight:600">{buttonText}</a>
              </p>
              <p style="font-size:13px;color:#6b7080">{footer}</p>
            </div>
            """;
    }
}
