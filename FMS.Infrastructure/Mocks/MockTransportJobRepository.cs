using FMS.Core.Models;
using FMS.Core.Repositories;
using FMS.Infrastructure.Database;
using FMS.JobManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMS.Infrastructure.Mocks;

public class MockTransportJobRepository : ITransportJobRepository
{
    private readonly List<IJob> _jobs = [];

    public Task AddAsync(IJob job)
    {
        _jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<IJob>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<IJob>>(_jobs);
    }

    public Task<IJob?> GetAsync(string jobId)
    {
        var job = _jobs.FirstOrDefault(j => j.JobId == jobId);
        return Task.FromResult(job);
    }

    public Task UpdateAsync(IJob job)
    {
        throw new NotImplementedException();
    }
}
