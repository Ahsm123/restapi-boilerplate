namespace Api.Exceptions;

public class EmailAlreadyTakenException : DomainException
{
    public string Email { get; }

    public EmailAlreadyTakenException(string email)
        : base($"Email '{email}' is already taken")
    {
        Email = email;
    }
}
