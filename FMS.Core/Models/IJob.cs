using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Core.Models;

public interface IJob
{
    string JobId { get; set; }
    string SourceOrderId { get; set; }
    string FromAddress { get; set; }
    string ToAddress { get; set; }
    string Status { get; set; }
    string? AssignedAgvId { get; set; }
    DateTime CreatedAt { get; set; }
    double DistanceMeters { get; set; }
    int EstimatedDurationSeconds { get; set; }
    string? RoutePolyline { get; set; }
    double? OriginLatitude { get; set; }
    double? OriginLongitude { get; set; }
    double? DestinationLatitude { get; set; }
    double? DestinationLongitude { get; set; }
}
