namespace FMS.JobManagement.Commands;

public sealed record CreateTransportJobCommand(
    string JobId,
    string OrderId,
    string FromAddress,
    string ToAddress) : ICommand<Unit>
{
    public string? TraceParent { get; init; }
    public string? TraceState { get; init; }
}
