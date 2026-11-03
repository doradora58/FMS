using FMS.Core.Models;

namespace FMS.JobManagement.Models;
public class Job : IJob
{
    public string JobId { get; set; } = null!;
    public string SourceOrderId { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddress { get; set; } = null!;
    public string Status { get; set; } = "QUEUED"; // QUEUED, ASSIGNED, INTRANSIT, COMPLETED
    public string? AssignedAgvId { get; set; }
    public DateTime CreatedAt { get; set; }
    public double DistanceMeters { get; set; }
    public int EstimatedDurationSeconds { get; set; }
}