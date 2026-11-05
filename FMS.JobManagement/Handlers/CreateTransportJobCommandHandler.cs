using System.Diagnostics;
using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Core.Services;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using FMS.JobManagement.Telemetry;
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
        using var activity = JobActivity.Source.StartActivity("job.create");
        activity?.SetTag("job.id", command.JobId);
        activity?.SetTag("order.id", command.OrderId);

        IJob? job;
        using (var loadActivity = JobActivity.Source.StartActivity("job.load"))
        {
            loadActivity?.SetTag("job.id", command.JobId);
            job = await jobRepository.GetAsync(command.JobId);
            loadActivity?.SetTag("job.found", job is not null);
        }

        if (job is null)
        {
            job = jobFactory.CreatePendingRoute(
                command.JobId,
                command.OrderId,
                command.FromAddress,
                command.ToAddress);
            using var insertActivity = JobActivity.Source.StartActivity("job.persist");
            insertActivity?.SetTag("job.id", job.JobId);
            await jobRepository.AddAsync(job);
        }
        else if (!string.Equals(job.Status, JobStatus.Routing.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            activity?.SetTag("job.duplicate", true);
            return Unit.Value;
        }

        try
        {
            using var routeActivity = JobActivity.Source.StartActivity("job.route.calculate", ActivityKind.Client);
            routeActivity?.SetTag("job.id", job.JobId);
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
            routeActivity?.SetTag("route.distance_meters", route.DistanceMeters);
        }
        catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
        {
            logger.LogError(exception, "Route calculation failed for job {JobId}.", job.JobId);
            job.Status = JobStatus.Failed.ToString();
            activity?.SetTag("error.type", exception.GetType().FullName);
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        }

        using (var updateActivity = JobActivity.Source.StartActivity("job.persist"))
        {
            updateActivity?.SetTag("job.id", job.JobId);
            updateActivity?.SetTag("job.status", job.Status);
            await jobRepository.UpdateAsync(job);
        }

        using (var publishActivity = JobActivity.Source.StartActivity("job.status.publish"))
        {
            publishActivity?.SetTag("job.id", job.JobId);
            publishActivity?.SetTag("job.status", job.Status);
            activity?.SetTag("job.status", job.Status);
            await messagePublisher.PublishJobStatusAsync(new JobStatusEvent(
                job.JobId,
                job.SourceOrderId,
                job.Status,
                DateTime.UtcNow));
        }

        return Unit.Value;
    }
}
