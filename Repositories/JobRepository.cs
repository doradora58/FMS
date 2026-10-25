using FMS.Models;
using static System.Reflection.Metadata.BlobBuilder;

namespace FMS.Repositories;
public class JobRepository : IJobRepository
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
}
