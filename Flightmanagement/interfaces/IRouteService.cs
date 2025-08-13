// Interfaces/IRouteService.cs
using Flightmanagement.Models;

namespace Flightmanagement.Interfaces;

public interface IRouteService
{
    Task<Route> CreateRouteAsync(int originAirportId, int destinationAirportId, int distanceKm);
    Task DeleteRouteAsync(int routeId); // guard if flights exist
    Task<List<Route>> ListWithAirportsAsync();
}
