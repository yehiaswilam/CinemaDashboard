namespace CinemaDashboard.Services.Email;

/// <summary>
/// SMTP settings, bound from the "EmailSettings" section of appsettings.json.
/// Leave UserName empty and emails are written to the console instead of being sent.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Cinema";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);
}
