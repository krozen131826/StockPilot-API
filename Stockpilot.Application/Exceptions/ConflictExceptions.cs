namespace Stockpilot.Application.Exceptions;

public class ConflictExceptions : Exception
{
    public ConflictExceptions(string message) : base(message)
    {
    }
}
