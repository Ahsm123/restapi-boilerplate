namespace Api.Exceptions;

public class EmailDeliveryException : DomainException
{
    public string EmailAddress { get; }

    public EmailDeliveryException(string emailAddress, string message, Exception? innerException = null)
            : base($"Failed to send email to {emailAddress}: {message}", innerException)
    {
        EmailAddress = emailAddress;
    }
}
