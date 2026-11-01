using FMS.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace FMS.Repositories;
public class TransportJobRepository : ITransportJobRepository
{
    private readonly List<Job> _jobs = [];
    public Task AddAsync(Job job)
    {
        _jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Job>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Job>>(_jobs);
    }

    public Task<Job?> GetAsync(string jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.JobId == jobId);
        return Task.FromResult(job);
    }
}
