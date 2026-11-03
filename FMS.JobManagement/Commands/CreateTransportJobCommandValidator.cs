namespace FMS.JobManagement.Commands;

public sealed class CreateTransportJobCommandValidator
    : ICommandValidator<CreateTransportJobCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(CreateTransportJobCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        AddRequiredError(errors, nameof(command.JobId), command.JobId);
        AddRequiredError(errors, nameof(command.OrderId), command.OrderId);
        AddRequiredError(errors, nameof(command.FromAddress), command.FromAddress);
        AddRequiredError(errors, nameof(command.ToAddress), command.ToAddress);
        return errors;
    }

    private static void AddRequiredError(
        IDictionary<string, string[]> errors,
        string field,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[field] = [$"{field} is required."];
        }
    }
}
