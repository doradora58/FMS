using FMS.Core.Models;
using FMS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Mocks;

public class MockTransportJobRepository : ITransportJobRepository
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
