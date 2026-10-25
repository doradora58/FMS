using FMS.Models;

namespace FMS.Services;
public interface IJobService
{
    Task<Job> CreateJobFromDispatchAsync(DispatchRequest req);
    Task<IEnumerable<Job>> GetJobsAsync();
}

