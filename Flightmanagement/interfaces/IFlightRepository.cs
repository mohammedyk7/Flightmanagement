using Flightmanagement.Models;

namespace Flightmanagement.Interfaces
{
    public interface IFlightRepository : IRepository<Flight>
    {
        Task<List<Flight>> GetInWindowAsync(DateTime fromUtc, DateTime toUtc);
        Task<int> CountSoldSeatsAsync(int flightId);
    }
}
