using Flightmanagement.Models;

namespace Flightmanagement.Interfaces;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(
        int passengerId,
        IEnumerable<(int flightId, string seat, decimal fare)> items);

    Task CheckInAsync(int bookingId);

    Task AddBaggageAsync(int bookingId, decimal weightKg, string tagNumber);

    Task CancelBookingAsync(int bookingId);
}
