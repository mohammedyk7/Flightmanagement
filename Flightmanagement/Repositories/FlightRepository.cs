namespace Flightmanagement.Repositories;

using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;

public class FlightRepository : Repository<Flight>, IFlightRepository
{
    public FlightRepository(FlightContext db) : base(db) { }

    public Task<List<Flight>> GetInWindowAsync(DateTime fromUtc, DateTime toUtc) =>
        _set.AsNoTracking()
            .Where(f => f.DepartureTime >= fromUtc && f.DepartureTime < toUtc)
            .Include(f => f.Route)
            .Include(f => f.Aircraft)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

    public Task<int> CountSoldSeatsAsync(int flightId) =>
        _db.Tickets.CountAsync(t => t.FlightId == flightId);
}
