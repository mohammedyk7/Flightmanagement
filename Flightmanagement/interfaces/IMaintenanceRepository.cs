using Flightmanagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Interfaces
{
    public interface IMaintenanceRepository
    {
        Task<List<AircraftMaintenance>> ListAsync();
        Task<AircraftMaintenance?> GetByIdAsync(int id);
        Task<List<AircraftMaintenance>> ListForAircraftAsync(int aircraftId);

        Task AddAsync(AircraftMaintenance entity);
        Task UpdateAsync(AircraftMaintenance entity);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
