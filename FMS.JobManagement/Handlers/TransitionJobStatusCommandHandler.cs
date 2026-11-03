using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using FMS.JobManagement.StateMachine;

namespace FMS.JobManagement.Handlers;

public sealed class TransitionJobStatusCommandHandler(
    ITransportJobRepository jobRepository,
    IMessagePublisher messagePublisher)
    : ICommandHandler<TransitionJobStatusCommand, Unit>
{
    public async Task<Unit> HandleAsync(
        TransitionJobStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetAsync(command.JobId)
            ?? throw new KeyNotFoundException($"Job {command.JobId} not found.");

        JobStatus currentStatus;
        if (string.Equals(job.Status, "Moving", StringComparison.OrdinalIgnoreCase))
        {
            currentStatus = JobStatus.InTransit;
        }
        else if (!Enum.TryParse<JobStatus>(job.Status, out currentStatus))
        {
            currentStatus = JobStatus.Queued;
        }

        var machine = new JobStateMachine(currentStatus);
        machine.Fire(command.Trigger);

        job.Status = machine.CurrentState.ToString();
        await jobRepository.UpdateAsync(job);
        await messagePublisher.PublishJobStatusAsync(new JobStatusEvent(
            job.JobId,
            job.SourceOrderId,
            job.Status,
            DateTime.UtcNow));

        return Unit.Value;
    }
}
