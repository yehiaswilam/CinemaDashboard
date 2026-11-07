using System.Text.RegularExpressions;

namespace CinemaDashboard.Services.Email;

/// <summary>
/// Used while EmailSettings isn't filled in: prints the email (and its links) to the console / Output window,
/// so confirmation and reset flows can still be tested locally.
/// </summary>
public partial class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody)
    {
        var links = LinkRegex().Matches(htmlBody).Select(m => m.Groups[1].Value).Distinct();

        logger.LogWarning(
            "\n===== EMAIL (not sent — EmailSettings is empty) =====\nTo: {To}\nSubject: {Subject}\nLinks:\n{Links}\n=====================================================",
            to, subject, string.Join("\n", links));

        return Task.CompletedTask;
    }

    [GeneratedRegex("href=\"([^\"]+)\"")]
    private static partial Regex LinkRegex();
}
