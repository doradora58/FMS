using FMS.Core.Models;
using FMS.JobManagement.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Database;

public class JobDocument: IJob
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string JobId { get; set; } = null!;
    public string SourceOrderId { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddress { get; set; } = null!;
    public double DistanceMeters { get; set; }
    public int EstimatedDurationSeconds { get; set; }

    public string Status { get; set; } = "QUEUED";
    public string? AssignedAgvId { get; set; }
    public DateTime CreatedAt { get; set; }

    public static JobDocument FromDomain(IJob job)
    {
        return new JobDocument
        {
            JobId = job.JobId,
            SourceOrderId = job.SourceOrderId,
            FromAddress = job.FromAddress,
            ToAddress = job.ToAddress,
            DistanceMeters = job.DistanceMeters,
            EstimatedDurationSeconds = job.EstimatedDurationSeconds,
            Status = job.Status,
            AssignedAgvId = job.AssignedAgvId,
            CreatedAt = job.CreatedAt
        };
    }

    public Job ToDomain()
    {
        return new Job
        {
            JobId = this.JobId,
            SourceOrderId = this.SourceOrderId,
            FromAddress = this.FromAddress,
            ToAddress = this.ToAddress,
            DistanceMeters = this.DistanceMeters,
            EstimatedDurationSeconds = this.EstimatedDurationSeconds,
            Status = this.Status,
            AssignedAgvId = this.AssignedAgvId,
            CreatedAt = this.CreatedAt
        };
    }
}
