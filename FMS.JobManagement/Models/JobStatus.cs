namespace FMS.JobManagement;
public enum JobStatus
{
    Queued,
    Assigned,
    Moving,
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
