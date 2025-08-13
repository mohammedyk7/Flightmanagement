// Interfaces/IFlightService.cs
namespace Flightmanagement.Interfaces;

public interface IFlightService
{
    Task<List<Flight>> GetFlightsInWindowAsync(DateTime fromUtc, DateTime toUtc);
    Task<int> GetAvailableSeatCountAsync(int flightId);
}
