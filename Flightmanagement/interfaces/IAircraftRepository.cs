using Flightmanagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Interfaces
{
    public interface IAircraftRepository
    {
        Task<List<Aircraft>> ListAsync();
        Task<Aircraft?> GetByIdAsync(int id);
        Task<Aircraft?> GetByTailNumberAsync(string tailNumber);

        Task AddAsync(Aircraft entity);
        Task UpdateAsync(Aircraft entity);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
