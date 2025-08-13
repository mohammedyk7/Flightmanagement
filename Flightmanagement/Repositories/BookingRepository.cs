namespace Flightmanagement.Repositories;

using Flightmanagement;
using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(FlightContext db) : base(db) { }

    public async Task<Booking> CreateWithTicketsAsync(Booking booking, IEnumerable<Ticket> tickets)
    {
        await _set.AddAsync(booking);
        await _db.SaveChangesAsync(); // ensures BookingId is generated

        foreach (var t in tickets)
            t.BookingId = booking.BookingId;

        await _db.Set<Ticket>().AddRangeAsync(tickets);
        await _db.SaveChangesAsync();

        return booking;
    }

    public async Task DeleteCascadeAsync(int bookingId)
    {
        var tickets = await _db.Set<Ticket>()
                               .Where(t => t.BookingId == bookingId)
                               .ToListAsync();
        _db.RemoveRange(tickets);

        var booking = await _set.FindAsync(bookingId);
        if (booking != null) _set.Remove(booking);

        await _db.SaveChangesAsync();
    }
}
