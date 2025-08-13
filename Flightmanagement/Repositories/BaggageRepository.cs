using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Flightmanagement.Repositories
{
    public class BaggageRepository : IBaggageRepository
    {
        private readonly FlightContext _db;
        public BaggageRepository(FlightContext db) => _db = db;

        public Task<List<Baggage>> ListAsync() =>
            _db.Baggage
               .Include(b => b.Ticket)
               .AsNoTracking()
               .ToListAsync();

        public async Task<Baggage?> GetByIdAsync(int id) =>
            await _db.Baggage.FindAsync(id); // works regardless of PK name

        // Your schema links baggage to Ticket, not Booking
        public Task<List<Baggage>> ListForTicketAsync(int ticketId) =>
            _db.Baggage
               .Where(b => b.TicketId == ticketId)
               .AsNoTracking()
               .ToListAsync();

        public async Task AddAsync(Baggage entity) => await _db.Baggage.AddAsync(entity);
        public Task UpdateAsync(Baggage entity) { _db.Baggage.Update(entity); return Task.CompletedTask; }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Baggage.FindAsync(id);
            if (existing != null) _db.Baggage.Remove(existing);
        }

        public Task SaveAsync() => _db.SaveChangesAsync();
    }
}
