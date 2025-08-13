// Repositories/AirportRepository.cs
namespace Flightmanagement.Repositories;

using Flightmanagement;
using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Microsoft.EntityFrameworkCore;

public class AirportRepository : Repository<Airport>, IAirportRepository
{
    public AirportRepository(FlightContext db) : base(db) { }

    public Task<Airport?> FindByIataAsync(string iata) =>
        _set.AsNoTracking().FirstOrDefaultAsync(a => a.IATA == iata);

    public Task<bool> HasAnyRoutesAsync(int airportId) =>
        _db.Routes.AnyAsync(r => r.OriginAirportId == airportId || r.DestinationAirportId == airportId);
}
