namespace Api.Configuration;

public class UrlSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ResetPasswordPath { get; set; } = "/reset-password";
    public string VerifyEmailPath { get; set; } = "/verify-email";
}
