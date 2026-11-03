using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using Microsoft.Extensions.Logging;

namespace FMS.JobManagement.Handlers;

public sealed class CreateTransportJobCommandHandler(
    ITransportJobRepository jobRepository,
    IJobFactory jobFactory,
    IRoutingService routingService,
    IMessagePublisher messagePublisher,
    ILogger<CreateTransportJobCommandHandler> logger)
    : ICommandHandler<CreateTransportJobCommand, Unit>
{
    public async Task<Unit> HandleAsync(
        CreateTransportJobCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetAsync(command.JobId);
        if (job is null)
        {
            job = jobFactory.CreatePendingRoute(
                command.JobId,
                command.OrderId,
                command.FromAddress,
                command.ToAddress);
            await jobRepository.AddAsync(job);
        }
        else if (!string.Equals(job.Status, JobStatus.Routing.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Unit.Value;
        }

        try
        {
            var route = await routingService.GetRouteAsync(
                job.FromAddress,
                job.ToAddress,
                cancellationToken);

            job.DistanceMeters = route.DistanceMeters;
            job.EstimatedDurationSeconds = route.DurationSeconds;
            job.RoutePolyline = route.EncodedPolyline;
            job.OriginLatitude = route.OriginLatitude;
            job.OriginLongitude = route.OriginLongitude;
            job.DestinationLatitude = route.DestinationLatitude;
            job.DestinationLongitude = route.DestinationLongitude;
            job.Status = JobStatus.Queued.ToString();
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            logger.LogError(exception, "Route calculation failed for job {JobId}.", job.JobId);
            job.Status = JobStatus.Failed.ToString();
        }

        await jobRepository.UpdateAsync(job);
        await messagePublisher.PublishJobStatusAsync(new JobStatusEvent(
            job.JobId,
            job.SourceOrderId,
            job.Status,
            DateTime.UtcNow));

        return Unit.Value;
    }
}
