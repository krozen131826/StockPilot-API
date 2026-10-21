using FluentValidation.Results;

namespace Stockpilot.Application.Exceptions;

public class ValidationExceptions : Exception
{
    public List<string> ValidationErrors { get; private set; }

    public ValidationExceptions(ValidationResult validationResult)
    {
        ValidationErrors = new List<string>();

        foreach (var validationError in validationResult.Errors)
        {
            ValidationErrors.Add(validationError.ErrorMessage);
        }
    }
}
