using FMS.Core.Models;
using FMS.JobManagement;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using FMS.JobManagement.Services;

namespace FMS.Facade;

public class TransportJobFacade(
    ICommandExecutor commandExecutor,
    ICommandValidator<CreateTransportJobCommand> createCommandValidator,
    IJobQueries jobQueries) : ITransportJobFacade
{
    public Task<CreateTransportJobCommand> CreateTransportJobAsync(
        TransportJobRequest req,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateTransportJobCommand(
            $"JOB-{Guid.NewGuid():N}".ToUpperInvariant(),
            req.OrderId,
            req.FromAddress,
            req.ToAddress);

        var errors = createCommandValidator.Validate(command);
        if (errors.Count > 0)
        {
            throw new CommandValidationException(errors);
        }

        return Task.FromResult(command);
    }

    public async Task<IEnumerable<IJob>> GetAllJobsAsync(CancellationToken cancellationToken = default)
    {
        return await jobQueries.GetAllJobsAsync(cancellationToken);
    }

    public async Task<IJob?> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return await jobQueries.GetJobAsync(jobId, cancellationToken);
    }

    public async Task ProgressJobAsync(string jobId, JobTrigger trigger, CancellationToken cancellationToken = default)
    {
        await commandExecutor.ExecuteAsync<TransitionJobStatusCommand, Unit>(
            new TransitionJobStatusCommand(jobId, trigger),
            cancellationToken);
    }
}
