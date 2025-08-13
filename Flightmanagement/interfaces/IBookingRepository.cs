// Interfaces/IBookingRepository.cs
using Flightmanagement.Models;

namespace Flightmanagement.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking> CreateWithTicketsAsync(Booking booking, IEnumerable<Ticket> tickets);
    Task DeleteCascadeAsync(int bookingId);
}
