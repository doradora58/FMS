using FMS.Core.Models;
using FMS.Core.Repositories;
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
    public async Task AddAsync(Job job)
    {
        var document = JobDocument.FromDomain(job);
        await _jobsCollection.InsertOneAsync(document);
        logger.LogInformation("[MongoDB] Job inserted: {JobId}", job.JobId);
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        var documents = await _jobsCollection.Find(_ => true).ToListAsync();
        return documents.Select(d => d.ToDomain());
    }

    public async Task<Job?> GetAsync(string jobId)
    {
        var document = await _jobsCollection.Find(j => j.JobId == jobId).FirstOrDefaultAsync();
        return document?.ToDomain();
    }
}
