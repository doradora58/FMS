using FMS.Core.Models;
using FMS.JobManagement;
using FMS.JobManagement.Models;

namespace FMS.Facade;

public interface ITransportJobFacade
{
    Task<IJob> CreateTransportJobAsync(TransportJobRequest req, CancellationToken cancellationToken = default);
    Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default);
    Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
    Task ProgressJobAsync(string jobId, JobTrigger trigger, CancellationToken cancellationToken = default);
}
