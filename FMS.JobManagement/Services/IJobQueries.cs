using FMS.Core.Models;

namespace FMS.JobManagement.Services;

public interface IJobQueries
{
    Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default);
    Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
}
