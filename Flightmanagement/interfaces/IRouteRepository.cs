// Interfaces/IRouteRepository.cs
namespace Flightmanagement.Interfaces;

using Flightmanagement.Models;
using System.Linq.Expressions;

public interface IRouteRepository : IRepository<Route>
{
    Task<Route?> FindAsync(int originAirportId, int destinationAirportId);
    Task<bool> ExistsAsync(int originAirportId, int destinationAirportId, int? excludeRouteId = null);
    Task<List<Route>> WithAirportsAsync(Expression<Func<Route, bool>>? predicate = null);
}
