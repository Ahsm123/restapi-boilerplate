namespace Api.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public SmtpSettings Smtp { get; set; } = new();
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public UrlSettings Urls { get; set; } = new();
}
