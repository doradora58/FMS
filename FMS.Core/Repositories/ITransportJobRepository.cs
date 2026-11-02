using FMS.Core.Models;
using FMS.Core.Repositories;

namespace FMS.Core.Repositories;
public interface ITransportJobRepository
{
    Task AddAsync(IJob job);
    Task<IEnumerable<IJob>> GetAllAsync();
    Task<IJob?> GetAsync(string jobId); 
    Task UpdateAsync(IJob job);
}