using System.Diagnostics;
using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.JobManagement.Commands;
using FMS.JobManagement.Models;
using FMS.JobManagement.StateMachine;
using FMS.JobManagement.Telemetry;

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
        using var activity = JobActivity.Source.StartActivity("job.status.transition");
        activity?.SetTag("job.id", command.JobId);
        activity?.SetTag("job.trigger", command.Trigger.ToString());

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
        using (var updateActivity = JobActivity.Source.StartActivity("job.persist"))
        {
            updateActivity?.SetTag("job.id", job.JobId);
            updateActivity?.SetTag("job.status", job.Status);
            await jobRepository.UpdateAsync(job);
        }

        using (var publishActivity = JobActivity.Source.StartActivity("job.status.publish"))
        {
            publishActivity?.SetTag("job.id", job.JobId);
            publishActivity?.SetTag("job.status", job.Status);
            activity?.SetTag("job.status", job.Status);
            await messagePublisher.PublishJobStatusAsync(new JobStatusEvent(
                job.JobId,
                job.SourceOrderId,
                job.Status,
                DateTime.UtcNow));
        }

        return Unit.Value;
    }
}
