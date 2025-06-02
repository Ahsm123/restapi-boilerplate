namespace Api.Exceptions;

public class EmailConfigurationException : DomainException
{
    public EmailConfigurationException(string message)
        : base($"Email configuration error: {message}")
    {
    }
}
