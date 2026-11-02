using FMS.Core.Models;
using FMS.Core.Repositories;


namespace FMS.Core.Services;
public interface ITransportJobService
{
    Task<Job> CreateTransportJobAsync(TransportJobRequest req, CancellationToken cancellationToken = default);
    Task<IEnumerable<Job>> GetAllJobsAsync();

    Task<Job?> GetJobAsync(string jobId);
}

