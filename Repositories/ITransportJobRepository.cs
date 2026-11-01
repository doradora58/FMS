using FMS.Models;

namespace FMS.Repositories;
public interface ITransportJobRepository
{
    Task AddAsync(Job job);
    Task<IEnumerable<Job>> GetAllAsync();
    Task<Job?> GetAsync(string jobId);
}