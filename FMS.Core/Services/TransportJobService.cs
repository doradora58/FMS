using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Infrastructure.Factories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using System.Threading;

namespace FMS.Core.Services;
public class TransportJobService(ITransportJobRepository jobRepository,IMessagePublisher publisher,IRoutingServiceFactory routingServiceFactory) : ITransportJobService
{
    public async Task<Job> CreateTransportJobAsync(TransportJobRequest request, CancellationToken cancellationToken = default)
    {
        var routingService = routingServiceFactory.Create();

        var routeInfo = await routingService.GetRouteAsync(request.FromAddress, request.ToAddress, cancellationToken);

        var job = new Job
        {
            JobId = $"JOB-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            SourceOrderId = request.OrderId,
            FromAddress = request.FromAddress,
            ToAddress = request.ToAddress,
            DistanceMeters = routeInfo.DistanceMeters,
            EstimatedDurationSeconds = routeInfo.DurationSeconds,
            Status = "QUEUED",
            CreatedAt = DateTime.UtcNow
        };

        await jobRepository.AddAsync(job); 

        var jobEvent = new JobStatusEvent(job.JobId, job.SourceOrderId, "Queued", DateTime.UtcNow);
        await publisher.PublishJobStatusAsync(jobEvent);

        return job;
    }

    public async Task<IEnumerable<Job>> GetAllJobsAsync()
    {
        return await jobRepository.GetAllAsync();
    }

    public async Task<Job?> GetJobAsync(string jobId)
    {
        return await jobRepository.GetAsync(jobId);
    }
}

