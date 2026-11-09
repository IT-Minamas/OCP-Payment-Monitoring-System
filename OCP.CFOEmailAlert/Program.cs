using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace OCP.CFOEmailAlert;

internal static class Program
{
    private static async Task<int> Main()
    {
        try
        {
            var settings = LoadSettings();
            ValidateSettings(settings);

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] CFO email alert started.");
            var memos = await GetPendingMemosAsync(settings.Database);
            Console.WriteLine($"Found {memos.Count} memo(s).");

            await SendEmailAsync(settings.Email, memos);
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Email sent successfully to {settings.Email.To}.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {ex}");
            return 1;
        }
    }

    private static AppSettings LoadSettings()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Configuration file not found: {path}");

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Unable to read appsettings.json.");
    }

    private static void ValidateSettings(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Database.ConnectionString))
            throw new InvalidOperationException("Database.ConnectionString is required.");
        if (string.IsNullOrWhiteSpace(settings.Email.SmtpHost))
            throw new InvalidOperationException("Email.SmtpHost is required.");
        if (string.IsNullOrWhiteSpace(settings.Email.From))
            throw new InvalidOperationException("Email.From is required.");
        if (string.IsNullOrWhiteSpace(settings.Email.To))
            throw new InvalidOperationException("Email.To is required.");
        if (settings.Email.SmtpPort <= 0)
            throw new InvalidOperationException("Email.SmtpPort must be greater than zero.");
    }

    private static async Task<List<MemoProgress>> GetPendingMemosAsync(DatabaseSettings config)
    {
        const string sql = @"
SELECT fldNo, MemoStatus
FROM dbo.vw_rpt_MemoProgress
WHERE ApprovedOn_L30 IS NOT NULL
  AND ApprovedOn_L40 IS NULL
ORDER BY ApprovedOn_L30, fldNo;";

        var result = new List<MemoProgress>();
        await using var connection = new SqlConnection(config.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = config.CommandTimeoutSeconds
        };
        await using var reader = await command.ExecuteReaderAsync();
        var memoNoOrdinal = reader.GetOrdinal("fldNo");
        var statusOrdinal = reader.GetOrdinal("MemoStatus");

        while (await reader.ReadAsync())
        {
            result.Add(new MemoProgress
            {
                MemoNo = reader.IsDBNull(memoNoOrdinal) ? string.Empty : reader.GetValue(memoNoOrdinal).ToString() ?? string.Empty,
                MemoStatus = reader.IsDBNull(statusOrdinal) ? string.Empty : reader.GetValue(statusOrdinal).ToString() ?? string.Empty
            });
        }

        return result;
    }

    private static async Task SendEmailAsync(EmailSettings config, IReadOnlyList<MemoProgress> memos)
    {
        var subject = $"OCP Payment Alert – Pending CFO Approval ({memos.Count} Memo)";
        var body = BuildEmailBody(memos);

        using var message = new MailMessage
        {
            From = new MailAddress(config.From, config.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };

        foreach (var address in config.To.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            message.To.Add(address);
        foreach (var address in config.Cc.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            message.CC.Add(address);

        using var smtp = new SmtpClient(config.SmtpHost, config.SmtpPort)
        {
            EnableSsl = config.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = config.TimeoutMilliseconds
        };

        if (!string.IsNullOrWhiteSpace(config.Username))
            smtp.Credentials = new NetworkCredential(config.Username, config.Password);
        else
            smtp.UseDefaultCredentials = false;

        await smtp.SendMailAsync(message);
    }

    private static string BuildEmailBody(IReadOnlyList<MemoProgress> memos)
    {
        var html = new StringBuilder();
        html.Append("<html><body style='font-family:Arial,sans-serif;font-size:10pt;color:#222'>");
        html.Append("<p>Dear CFO,</p>");
        html.Append("<p>Please find below the OCP payment memos that have completed the RCEO approval stage (L30) and are still pending CFO approval (L40).</p>");
        html.Append($"<p><b>Total pending memo(s): {memos.Count}</b></p>");

        if (memos.Count == 0)
        {
            html.Append("<p>There are currently no memos pending CFO approval.</p>");
        }
        else
        {
            html.Append("<table cellpadding='7' cellspacing='0' style='border-collapse:collapse;border:1px solid #ccc'>");
            html.Append("<thead><tr style='background-color:#eaf0f6'><th style='border:1px solid #ccc;text-align:left'>No.</th><th style='border:1px solid #ccc;text-align:left'>Memo No.</th><th style='border:1px solid #ccc;text-align:left'>Memo Status</th></tr></thead><tbody>");
            for (var i = 0; i < memos.Count; i++)
            {
                html.Append("<tr>");
                html.Append($"<td style='border:1px solid #ccc'>{i + 1}</td>");
                html.Append($"<td style='border:1px solid #ccc'>{WebUtility.HtmlEncode(memos[i].MemoNo)}</td>");
                html.Append($"<td style='border:1px solid #ccc'>{WebUtility.HtmlEncode(memos[i].MemoStatus)}</td>");
                html.Append("</tr>");
            }
            html.Append("</tbody></table>");
        }

        html.Append("<p>This is an automated notification from OCP Payment System. Please do not reply to this email.</p>");
        html.Append("<p>Regards,<br/>OCP Payment System</p>");
        html.Append("</body></html>");
        return html.ToString();
    }
}

internal sealed class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
}

internal sealed class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 60;
}

internal sealed class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 25;
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "OCP Payment System";
    public string To { get; set; } = "rizky.y.pratomo@gmail.com";
    public string Cc { get; set; } = string.Empty;
    public int TimeoutMilliseconds { get; set; } = 30000;
}

internal sealed class MemoProgress
{
    public string MemoNo { get; set; } = string.Empty;
    public string MemoStatus { get; set; } = string.Empty;
}
