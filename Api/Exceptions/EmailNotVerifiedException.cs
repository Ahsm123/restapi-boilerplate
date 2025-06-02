namespace Api.Exceptions;

public class EmailNotVerifiedException : DomainException
{
    public EmailNotVerifiedException()
        : base("Please verify your email before logging in")
    {
    }
}
