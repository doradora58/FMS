using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;

namespace FMS.JobManagement.StateMachine;

public class JobStateMachine
{
    private readonly StateMachine<JobStatus, JobTrigger> _machine;

    public JobStatus CurrentState => _machine.State;

    public JobStateMachine(JobStatus initialStatus)
    {
        _machine = new StateMachine<JobStatus, JobTrigger>(initialStatus);

        _machine.Configure(JobStatus.Queued)
            .Permit(JobTrigger.AssignAgv, JobStatus.Assigned)
            .Permit(JobTrigger.Cancel, JobStatus.Cancelled);

        _machine.Configure(JobStatus.Assigned)
            .Permit(JobTrigger.StartMoving, JobStatus.Moving)
            .Permit(JobTrigger.Cancel, JobStatus.Cancelled);

        _machine.Configure(JobStatus.Moving)
            .Permit(JobTrigger.Complete, JobStatus.Completed)
            .Permit(JobTrigger.Fail, JobStatus.Failed);

    }
    public void Fire(JobTrigger trigger)
    {
        if (_machine.CanFire(trigger))
        {
            _machine.Fire(trigger);
        }
        else
        {
            throw new InvalidOperationException($"Cannot fire trigger {trigger} from current state {_machine.State}.");
        }
    }

    public bool CanFire(JobTrigger trigger) => _machine.CanFire(trigger);
}
