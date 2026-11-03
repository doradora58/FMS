using FMS.JobManagement.Models;

namespace FMS.JobManagement.Commands;

public sealed record TransitionJobStatusCommand(
    string JobId,
    JobTrigger Trigger) : ICommand<Unit>;
