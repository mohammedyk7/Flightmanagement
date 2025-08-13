// Services/FlightService.cs
namespace Flightmanagement.Services;

using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Microsoft.EntityFrameworkCore;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _flights;
    private readonly FlightContext _db; // used only for counting tickets quickly

    public FlightService(IFlightRepository flights, FlightContext db)
    {
        _flights = flights;
        _db = db;
    }

    public Task<List<Flight>> GetFlightsInWindowAsync(DateTime fromUtc, DateTime toUtc)
        => _flights.GetInWindowAsync(fromUtc, toUtc);

    public Task<int> GetAvailableSeatCountAsync(int flightId)
        => _db.Tickets.CountAsync(t => t.FlightId == flightId);
}
