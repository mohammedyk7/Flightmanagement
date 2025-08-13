using Flightmanagement.Models;

namespace Flightmanagement.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<bool> IsSeatAvailableAsync(int flightId, string seat);

    // NEW:
    Task<List<Ticket>> ByBookingAsync(int bookingId);
    Task AddBaggageAsync(int ticketId, decimal weightKg, string tagNumber);
}
