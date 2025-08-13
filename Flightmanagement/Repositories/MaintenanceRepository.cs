using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flightmanagement.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly FlightContext _db;
        private DbSet<AircraftMaintenance> Set => _db.Set<AircraftMaintenance>();

        public MaintenanceRepository(FlightContext db) => _db = db;

        public Task<List<AircraftMaintenance>> ListAsync() =>
            Set.Include(m => m.Aircraft).AsNoTracking().ToListAsync();

        public Task<AircraftMaintenance?> GetByIdAsync(int id) =>
            Set.FindAsync(id).AsTask();

        public Task<List<AircraftMaintenance>> ListForAircraftAsync(int aircraftId) =>
            Set.Where(m => m.AircraftId == aircraftId)
               .AsNoTracking()
               .ToListAsync();

        public Task AddAsync(AircraftMaintenance entity) =>
            Set.AddAsync(entity).AsTask();

        public Task UpdateAsync(AircraftMaintenance entity)
        {
            Set.Update(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await Set.FindAsync(id);
            if (existing != null) Set.Remove(existing);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
