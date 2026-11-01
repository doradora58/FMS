using FMS.Core.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Database;

public class JobDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string JobId { get; set; } = null!;
    public string SourceOrderId { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string ToAddress { get; set; } = null!;
    public string Status { get; set; } = "QUEUED";
    public string? AssignedAgvId { get; set; }
    public DateTime CreatedAt { get; set; }

    public static JobDocument FromDomain(Job job)
    {
        return new JobDocument
        {
            JobId = job.JobId,
            SourceOrderId = job.SourceOrderId,
            FromAddress = job.FromAddress,
            ToAddress = job.ToAddress,
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
            Status = this.Status,
            AssignedAgvId = this.AssignedAgvId,
            CreatedAt = this.CreatedAt
        };
    }
}
