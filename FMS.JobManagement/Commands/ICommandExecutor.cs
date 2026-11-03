namespace FMS.JobManagement.Commands;

public interface ICommandExecutor
{
    Task EnqueueAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<Unit>;

    Task<TResult> ExecuteAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;
}
