namespace Api.Exceptions;

public class WeakPasswordException : DomainException
{
    public WeakPasswordException()
        : base("Password must contain at least one letter and one number")
    {
    }
}
