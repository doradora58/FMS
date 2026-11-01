using FMS.Models;
using FMS.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;

namespace FMS.Services;
public class TransportJobService(ITransportJobRepository jobRepository,IMessagePublisher publisher) : ITransportJobService
{
    public async Task<Job> CreateTransportJobAsync(TransportJobRequest req)
    {
        var job = new Job
        {
            JobId = $"JOB-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            SourceOrderId = req.OrderId,
            FromAddress = req.FromAddress,
            ToAddress = req.ToAddress,
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

