using Flightmanagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Interfaces
{
    public interface IPassengerRepository
    {
        Task<List<Passenger>> ListAsync();
        Task<Passenger?> GetByIdAsync(int id);
        Task<Passenger?> GetByPassportNoAsync(string passportNo);
        Task<List<Booking>> GetBookingsAsync(int passengerId);

        Task AddAsync(Passenger entity);
        Task UpdateAsync(Passenger entity);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
