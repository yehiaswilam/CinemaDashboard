namespace CinemaDashboard.Services.Email;

/// <summary>
/// Sends an email. Two implementations (LSP + DIP): SmtpEmailService really sends it,
/// ConsoleEmailService just logs it. Program.cs picks one based on appsettings.
/// </summary>
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
}
