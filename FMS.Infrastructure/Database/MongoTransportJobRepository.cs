using FMS.Core.Models;
using FMS.Core.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Database;

public class MongoTransportJobRepository(IConfiguration configuration,ILogger<MongoTransportJobRepository> logger) : ITransportJobRepository
{

    private readonly IMongoCollection<Job> _jobsCollection =
            new MongoClient(configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017")
                .GetDatabase(configuration["MongoDB:DatabaseName"] ?? "FmsDb")
                .GetCollection<Job>("Jobs");

    public async Task AddAsync(Job job)
    {
        await _jobsCollection.InsertOneAsync(job);
        logger.LogInformation("[MongoDB] Job inserted: {JobId}", job.JobId);
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _jobsCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Job?> GetAsync(string jobId)
    {
        return await _jobsCollection.Find(j => j.JobId == jobId).FirstOrDefaultAsync();
    }
}
