namespace FMS.JobManagement.Commands;

public sealed class CommandValidationException(
    IReadOnlyDictionary<string, string[]> errors)
    : Exception("The command failed validation.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
