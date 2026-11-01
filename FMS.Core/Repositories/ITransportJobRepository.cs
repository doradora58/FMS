using FMS.Core.Models;
using FMS.Core.Repositories;

namespace FMS.Core.Repositories;
public interface ITransportJobRepository
{
    Task AddAsync(Job job);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<Job?> GetAsync(string jobId);
}