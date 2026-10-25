using FMS.Models;

namespace FMS.Repositories;
public interface IJobRepository
{
    Task AddAsync(Job job);
    Task<IEnumerable<Job>> GetAllAsync();
}