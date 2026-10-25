using FMS.Models;
using FMS.Repositories;

namespace FMS.Services;
public class JobService(IJobRepository jobRepository) : IJobService
{
    public async Task<Job> CreateJobFromDispatchAsync(DispatchRequest req)
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

    public async Task<IEnumerable<Job>> GetJobsAsync()
    {
        return await jobRepository.GetAllAsync();
    }
}

