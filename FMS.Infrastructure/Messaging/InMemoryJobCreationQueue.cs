using System.Threading.Channels;
using System.Diagnostics;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Telemetry;
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
                var parentContext = default(ActivityContext);
                if (!string.IsNullOrWhiteSpace(command.TraceParent) &&
                    !ActivityContext.TryParse(
                        command.TraceParent,
                        command.TraceState,
                        isRemote: true,
                        out parentContext))
                {
                    throw new FormatException(
                        $"Invalid trace context on create command for job {command.JobId}.");
                }

                using var activity = JobActivity.Source.StartActivity(
                    "job.create.process",
                    ActivityKind.Consumer,
                    parentContext);
                activity?.SetTag("job.id", command.JobId);
                activity?.SetTag("order.id", command.OrderId);
                activity?.SetTag("messaging.system", "in-memory");
                activity?.SetTag("messaging.operation.type", "process");

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
