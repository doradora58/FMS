using FMS.Core.Models;
using FMS.Core.Services;
using FMS.JobManagement;
using FMS.JobManagement.Models;
using FMS.JobManagement.Services;

namespace FMS.Facade;

public class TransportJobFacade(IRoutingService routingService,
    IJobManagement jobManagement) : ITransportJobFacade
{
    public async Task<IJob> CreateTransportJobAsync(TransportJobRequest req, CancellationToken cancellationToken = default)
    {
        var routeInfo = await routingService.GetRouteAsync(req.FromAddress, req.ToAddress, cancellationToken);

        var job = await jobManagement.CreateAndSaveJobAsync(
            req.OrderId,
            req.FromAddress,
            req.ToAddress,
            routeInfo.DistanceMeters,
            routeInfo.DurationSeconds,
            cancellationToken
        );

        return job;
    }

    public async Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        return await jobManagement.GetAllJobsAsync(cancellationToken);
    }

    public async Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await jobManagement.GetJobAsync(jobId, cancellationToken);
    }

    public async Task ProgressJobAsync(string jobId, JobTrigger trigger, CancellationToken cancellationToken = default)
    {
        await jobManagement.TransitionStatusAsync(jobId, trigger, cancellationToken);
    }
}
