// Interfaces/IAirportRepository.cs
namespace Flightmanagement.Interfaces;

public interface IAirportRepository : IRepository<Airport>
{
    Task<Airport?> FindByIataAsync(string iata);
    Task<bool> HasAnyRoutesAsync(int airportId);
}
