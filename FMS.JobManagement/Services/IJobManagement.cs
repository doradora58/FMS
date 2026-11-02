using FMS.Core.Models;
using FMS.JobManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.JobManagement.Services;

public interface IJobManagement
{
    Task<IJob> CreateAndSaveJobAsync(string sourceOrderId, string fromAddress, string toAddress, double distance, int duration, CancellationToken cancellationToken = default);
    Task TransitionStatusAsync(string jobId, JobTrigger trigger, CancellationToken cancellationToken = default);
    Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default);
    Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default);
}
