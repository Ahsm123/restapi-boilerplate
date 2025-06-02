﻿namespace Api.Models;

public class EmailTemplate
{
    public string Subject { get; set; } = string.Empty;
    public string PlainTextBody { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
}
