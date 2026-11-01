using FMS.Models;

namespace FMS.Services;
public interface ITransportJobService
{
    Task<Job> CreateTransportJobAsync(TransportJobRequest req);
    Task<IEnumerable<Job>> GetAllJobsAsync();

    Task<Job?> GetJobAsync(string jobId);
}

