namespace Api.Exceptions;

public class ValidationException : DomainException
{
    public string Field { get; }

    public ValidationException(string field, string message)
        : base($"Validation failed for field '{field}': {message}")
    {
        Field = field;
    }
}
