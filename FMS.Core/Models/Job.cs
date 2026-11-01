namespace FMS.Core.Models;
public class Job
{
    public string JobId { get; set; } = null!;
    public string SourceOrderId { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddress { get; set; } = null!;
    public string Status { get; set; } = "QUEUED"; // QUEUED, ASSIGNED, RUNNING, COMPLETED
    public string? AssignedAgvId { get; set; }
    public DateTime CreatedAt { get; set; }
}