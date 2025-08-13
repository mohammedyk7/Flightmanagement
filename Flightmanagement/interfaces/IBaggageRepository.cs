using Flightmanagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Interfaces
{
    public interface IBaggageRepository
    {
        Task<List<Baggage>> ListAsync();
        Task<Baggage?> GetByIdAsync(int id);
        Task<List<Baggage>> ListForTicketAsync(int ticketId);

        Task AddAsync(Baggage entity);
        Task UpdateAsync(Baggage entity);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
