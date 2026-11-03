using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.JobManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Database;

public class MongoTransportJobRepository(IConfiguration configuration,ILogger<MongoTransportJobRepository> logger) : ITransportJobRepository
{

    private readonly IMongoCollection<JobDocument> _jobsCollection = CreateCollection(configuration);
    private static IMongoCollection<JobDocument> CreateCollection(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "FmsDb";

        var settings = MongoClientSettings.FromConnectionString(connectionString);

        settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());

        var client = new MongoClient(settings);
        var database = client.GetDatabase(databaseName);
        return database.GetCollection<JobDocument>("Jobs");
    }
    public async Task AddAsync(IJob job)
    {
        var document = JobDocument.FromDomain(job);
        await _jobsCollection.InsertOneAsync(document);
        logger.LogInformation("[MongoDB] Job inserted: {JobId}", job.JobId);
    }

    public async Task<IEnumerable<IJob>> GetAllAsync()
    {
        var documents = await _jobsCollection.Find(_ => true).ToListAsync();
        return documents.Select(d => d.ToDomain());
    }

    public async Task<IJob?> GetAsync(string jobId)
    {
        var document = await _jobsCollection.Find(j => j.JobId == jobId).FirstOrDefaultAsync();
        return document?.ToDomain();
    }

    public async Task UpdateAsync(IJob job)
    {
        var filter = Builders<JobDocument>.Filter.Eq(d => d.JobId, job.JobId);
        var update = Builders<JobDocument>.Update.Combine(
            Builders<JobDocument>.Update.Set(document => document.JobId, job.JobId),
            Builders<JobDocument>.Update.Set(document => document.SourceOrderId, job.SourceOrderId),
            Builders<JobDocument>.Update.Set(document => document.FromAddress, job.FromAddress),
            Builders<JobDocument>.Update.Set(document => document.ToAddress, job.ToAddress),
            Builders<JobDocument>.Update.Set(document => document.DistanceMeters, job.DistanceMeters),
            Builders<JobDocument>.Update.Set(document => document.EstimatedDurationSeconds, job.EstimatedDurationSeconds),
            Builders<JobDocument>.Update.Set(document => document.RoutePolyline, job.RoutePolyline),
            Builders<JobDocument>.Update.Set(document => document.OriginLatitude, job.OriginLatitude),
            Builders<JobDocument>.Update.Set(document => document.OriginLongitude, job.OriginLongitude),
            Builders<JobDocument>.Update.Set(document => document.DestinationLatitude, job.DestinationLatitude),
            Builders<JobDocument>.Update.Set(document => document.DestinationLongitude, job.DestinationLongitude),
            Builders<JobDocument>.Update.Set(document => document.Status, job.Status),
            Builders<JobDocument>.Update.Set(document => document.AssignedAgvId, job.AssignedAgvId),
            Builders<JobDocument>.Update.Set(document => document.CreatedAt, job.CreatedAt));

        var result = await _jobsCollection.UpdateOneAsync(filter, update);
        if (result.MatchedCount == 0)
        {
            throw new KeyNotFoundException($"Job {job.JobId} not found.");
        }
    }
}
