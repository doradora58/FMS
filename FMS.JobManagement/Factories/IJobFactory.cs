using FMS.JobManagement.Models;

namespace FMS.Core.Models;
public interface IJobFactory
{
    IJob CreatePendingRoute(string jobId, string sourceOrderId, string fromAddress, string toAddress);
    IJob Create(string sourceOrderId, string fromAddress, string toAddress, double distanceMeters, int durationSeconds);
}