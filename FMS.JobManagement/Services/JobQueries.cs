using FMS.Core.Models;
using FMS.Core.Repositories;

namespace FMS.JobManagement.Services;

public sealed class JobQueries(ITransportJobRepository jobRepository) : IJobQueries
{
    public Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        return jobRepository.GetAllAsync();
    }

    public Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return jobRepository.GetAsync(jobId);
    }
}
