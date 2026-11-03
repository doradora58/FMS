using FMS.Core.Models;

namespace FMS.JobManagement.Models;
public class Job : IJob
{
    public string JobId { get; set; } = null!;
    public string SourceOrderId { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddress { get; set; } = null!;
    public string Status { get; set; } = "QUEUED"; // ROUTING, QUEUED, ASSIGNED, INTRANSIT, COMPLETED, FAILED, CANCELLED
    public string? AssignedAgvId { get; set; }
    public DateTime CreatedAt { get; set; }
    public double DistanceMeters { get; set; }
    public int EstimatedDurationSeconds { get; set; }
    public string? RoutePolyline { get; set; }
    public double? OriginLatitude { get; set; }
    public double? OriginLongitude { get; set; }
    public double? DestinationLatitude { get; set; }
    public double? DestinationLongitude { get; set; }
}