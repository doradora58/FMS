using System.Threading.Channels;
using FMS.JobManagement.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FMS.Infrastructure.Messaging;

public sealed class InMemoryJobCreationQueue(
    ICommandExecutor executor,
    ILogger<InMemoryJobCreationQueue> logger)
    : BackgroundService, ICommandQueue<CreateTransportJobCommand>
{
    private readonly Channel<CreateTransportJobCommand> _channel = Channel.CreateUnbounded<CreateTransportJobCommand>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public async Task EnqueueAsync(
        CreateTransportJobCommand command,
        CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(command, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await executor.ExecuteAsync<CreateTransportJobCommand, Unit>(command, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process in-memory job creation for {JobId}.", command.JobId);
            }
        }
    }
}
