namespace Api.Exceptions;

public class UserNotFoundException : DomainException
{
    public Guid UserId { get; }

    public UserNotFoundException(Guid userId)
        : base($"User with ID {userId} not found.")
    {
        UserId = userId;
    }
}
