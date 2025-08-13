// Services/RouteService.cs
namespace Flightmanagement.Services;

using Flightmanagement.Interfaces;
using Flightmanagement.Models;

public class RouteService : IRouteService
{
    private readonly IRouteRepository _routes;
    private readonly IFlightRepository _flights;

    public RouteService(IRouteRepository routes, IFlightRepository flights)
    {
        _routes = routes;
        _flights = flights;
    }

    public async Task<Route> CreateRouteAsync(int originAirportId, int destinationAirportId, int distanceKm)
    {
        if (originAirportId == destinationAirportId)
            throw new InvalidOperationException("Origin and destination must differ.");

        var dup = await _routes.ExistsAsync(originAirportId, destinationAirportId);
        if (dup) throw new InvalidOperationException("Route already exists.");

        var r = new Route
        {
            OriginAirportId = originAirportId,
            DestinationAirportId = destinationAirportId,
            DistanceKm = distanceKm
        };
        await _routes.AddAsync(r);
        await _routes.SaveAsync();
        return r;
    }

    public async Task DeleteRouteAsync(int routeId)
    {
        var route = await _routes.GetByIdAsync(routeId) ?? throw new InvalidOperationException("Route not found.");

        // Block delete if flights reference this route
        var anyFlights = (await _flights.ListAsync(f => f.RouteId == routeId)).Any();
        if (anyFlights) throw new InvalidOperationException("Cannot delete route with scheduled flights.");

        _routes.Delete(route);
        await _routes.SaveAsync();
    }

    public Task<List<Route>> ListWithAirportsAsync() => _routes.WithAirportsAsync();
}
