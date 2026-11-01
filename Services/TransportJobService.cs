using FMS.Models;
using FMS.Repositories;

namespace FMS.Services;
public class TransportJobService(ITransportJobRepository jobRepository) : ITransportJobService
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

