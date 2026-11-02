using FMS.JobManagement;
using FMS.JobManagement.Models;

namespace FMS.Core.Models;
public class JobFactory: IJobFactory
{

    public IJob Create(string sourceOrderId, string fromAddress, string toAddress, double distanceMeters, int durationSeconds)
    {
        return new Job
        {
            JobId = $"JOB-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            SourceOrderId = sourceOrderId,
            FromAddress = fromAddress,
            ToAddress = toAddress,
            DistanceMeters = distanceMeters,
            EstimatedDurationSeconds = durationSeconds,
            Status = JobStatus.Queued.ToString(),
            CreatedAt = DateTime.UtcNow
        };
    }
}