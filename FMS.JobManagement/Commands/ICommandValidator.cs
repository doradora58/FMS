namespace FMS.JobManagement.Commands;

public interface ICommandValidator<in TCommand>
{
    IReadOnlyDictionary<string, string[]> Validate(TCommand command);
}
