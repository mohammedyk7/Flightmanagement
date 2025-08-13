using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Repositories
{
    public class AircraftRepository : IAircraftRepository
    {
        private readonly FlightContext _db;
        public AircraftRepository(FlightContext db) => _db = db;

        public Task<List<Aircraft>> ListAsync() =>
            _db.Aircraft.AsNoTracking().ToListAsync();

        public async Task<Aircraft?> GetByIdAsync(int id) =>
            await _db.Aircraft.FindAsync(id);

        public Task<Aircraft?> GetByTailNumberAsync(string tailNumber) =>
            _db.Aircraft.FirstOrDefaultAsync(a => a.TailNumber == tailNumber);

        public async Task AddAsync(Aircraft entity) => await _db.Aircraft.AddAsync(entity);
        public Task UpdateAsync(Aircraft entity) { _db.Aircraft.Update(entity); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Aircraft.FindAsync(id);
            if (existing != null) _db.Aircraft.Remove(existing);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
