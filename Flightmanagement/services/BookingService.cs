namespace Flightmanagement.Services;

using Flightmanagement;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookings;
    private readonly ITicketRepository _tickets;

    public BookingService(IBookingRepository bookings, ITicketRepository tickets)
    {
        _bookings = bookings;
        _tickets = tickets;
    }

    public async Task<Booking> CreateBookingAsync(
        int passengerId,
        IEnumerable<(int flightId, string seat, decimal fare)> items)
    {
        // 1) validate seat availability
        foreach (var (flightId, seat, _) in items)
        {
            var ok = await _tickets.IsSeatAvailableAsync(flightId, seat);
            if (!ok)
                throw new InvalidOperationException(
                    $"Seat {seat} on flight {flightId} is already booked.");
        }

        // 2) create booking
        var booking = new Booking
        {
            PassengerId = passengerId,
            BookingDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        // 3) create tickets
        var tickets = items.Select(i => new Ticket
        {
            FlightId = i.flightId,
            SeatNumber = i.seat,   // change to .Seat if that’s your property
            Fare = i.fare
        }).ToList();

        // 4) save booking + tickets (helper in repository)
        return await _bookings.CreateWithTicketsAsync(booking, tickets);
    }

    public async Task CheckInAsync(int bookingId)
    {
        var booking = await _bookings.GetByIdAsync(bookingId)
                      ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

        booking.Status = "CheckedIn";
        _bookings.Update(booking);
        await _bookings.SaveAsync();

        // mark all tickets as checked in
        var tix = await _tickets.ByBookingAsync(bookingId);
        foreach (var t in tix)
        {
            t.CheckedIn = true;              // adjust name if needed
            _tickets.Update(t);
        }
        await _tickets.SaveAsync();
    }

    public async Task AddBaggageAsync(int bookingId, decimal weightKg, string tagNumber)
    {
        var tix = await _tickets.ByBookingAsync(bookingId);
        var first = tix.FirstOrDefault()
                   ?? throw new InvalidOperationException("No tickets found for this booking.");

        await _tickets.AddBaggageAsync(first.TicketId, weightKg, tagNumber);
    }

    public Task CancelBookingAsync(int bookingId)
        => _bookings.DeleteCascadeAsync(bookingId);
}
