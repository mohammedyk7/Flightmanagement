// Repositories/RouteRepository.cs
namespace Flightmanagement.Repositories;

using Flightmanagement;
using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(FlightContext db) : base(db) { }

    public Task<Route?> FindAsync(int originId, int destId) =>
        _set.AsNoTracking().FirstOrDefaultAsync(r =>
            r.OriginAirportId == originId && r.DestinationAirportId == destId);

    public Task<bool> ExistsAsync(int originId, int destId, int? excludeRouteId = null) =>
        _set.AnyAsync(r =>
            r.OriginAirportId == originId &&
            r.DestinationAirportId == destId &&
            (!excludeRouteId.HasValue || r.RouteId != excludeRouteId.Value));

    // safe version: no Includes, sorts by IDs
    public Task<List<Route>> WithAirportsAsync(Expression<Func<Route, bool>>? predicate = null)
    {
        IQueryable<Route> q = _set.AsNoTracking();
        if (predicate != null) q = q.Where(predicate);
        return q.OrderBy(r => r.OriginAirportId)
                .ThenBy(r => r.DestinationAirportId)
                .ToListAsync();
    }
}
