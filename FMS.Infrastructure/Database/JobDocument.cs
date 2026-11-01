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

    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public static JobDocument FromDomain(Job job)
    {
        return new JobDocument
        {
            JobId = job.JobId,
            Status = job.Status
        };
    }

    public Job ToDomain()
    {
        return new Job
        {
            JobId = this.JobId,
            Status = this.Status
        };
    }
}
