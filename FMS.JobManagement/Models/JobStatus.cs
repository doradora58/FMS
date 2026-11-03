namespace FMS.JobManagement;
public enum JobStatus
{
    Routing,
    Queued,
    Assigned,
    InTransit,
    Completed,
    Failed,
    Cancelled
}

public enum JobTrigger
{
    AssignAgv,
    StartMoving,
    Complete,
    Fail,
    Cancel
}
