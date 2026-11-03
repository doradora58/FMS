using Microsoft.Extensions.DependencyInjection;

namespace FMS.JobManagement.Commands;

public sealed class CommandExecutor(
    IServiceProvider services,
    IServiceScopeFactory scopeFactory) : ICommandExecutor
{
    public Task EnqueueAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<Unit>
    {
        var queue = services.GetService(typeof(ICommandQueue<TCommand>))
            as ICommandQueue<TCommand>;

        if (queue is null)
        {
            throw new InvalidOperationException(
                $"No command queue is registered for {typeof(TCommand).FullName}.");
        }

        return queue.EnqueueAsync(command, cancellationToken);
    }

    public async Task<TResult> ExecuteAsync<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetService(typeof(ICommandHandler<TCommand, TResult>))
            as ICommandHandler<TCommand, TResult>;

        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No command handler is registered for {typeof(TCommand).FullName}.");
        }

        return await handler.HandleAsync(command, cancellationToken);
    }
}
