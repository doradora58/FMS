namespace FMS.JobManagement.Commands;

public sealed record CreateTransportJobCommand(
    string JobId,
    string OrderId,
    string FromAddress,
    string ToAddress) : ICommand<Unit>;
