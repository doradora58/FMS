namespace FMS.JobManagement.Commands;

public interface ICommandQueue<in TCommand>
{
    Task EnqueueAsync(TCommand command, CancellationToken cancellationToken = default);
}
