using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Repositories
{
    public class PassengerRepository : IPassengerRepository
    {
        private readonly FlightContext _db;
        public PassengerRepository(FlightContext db) => _db = db;

        public Task<List<Passenger>> ListAsync() =>
            _db.Passengers.AsNoTracking().ToListAsync();

        // Use FindAsync so we don’t care whether the PK is Id or PassengerId
        public async Task<Passenger?> GetByIdAsync(int id) =>
            await _db.Passengers.FindAsync(id);

        public Task<Passenger?> GetByPassportNoAsync(string passportNo) =>
            _db.Passengers.FirstOrDefaultAsync(p => p.PassportNo == passportNo);

        public Task<List<Booking>> GetBookingsAsync(int passengerId) =>
            _db.Bookings
               .Include(b => b.Tickets)
                   .ThenInclude(t => t.Flight)
               .Where(b => b.PassengerId == passengerId)
               .AsNoTracking()
               .ToListAsync();

        public async Task AddAsync(Passenger entity) => await _db.Passengers.AddAsync(entity);
        public Task UpdateAsync(Passenger entity) { _db.Passengers.Update(entity); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Passengers.FindAsync(id);
            if (existing != null) _db.Passengers.Remove(existing);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
