using Flightmanagement.Models;

public interface IAirportService
{
    Task<Airport> CreateAirportAsync(string iata, string name, string city, string country, string timeZone);
    Task DeleteAirportAsync(int airportId);   // guard if any routes exist
}
