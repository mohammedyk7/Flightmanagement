namespace Flightmanagement.Repositories;

using Flightmanagement;
using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(FlightContext db) : base(db) { }

    public async Task<bool> IsSeatAvailableAsync(int flightId, string seat)
    {
        // adjust property name if your model uses Seat instead of SeatNumber
        bool taken = await _set.AsNoTracking()
                               .AnyAsync(t => t.FlightId == flightId && t.SeatNumber == seat);
        return !taken;
    }

    public Task<List<Ticket>> ByBookingAsync(int bookingId) =>
        _set.Where(t => t.BookingId == bookingId).ToListAsync();

    public async Task AddBaggageAsync(int ticketId, decimal weightKg, string tagNumber)
    {
        var baggage = new Baggage
        {
            TicketId = ticketId,
            WeightKg = weightKg,
            TagNumber = tagNumber
        };

        await _db.Set<Baggage>().AddAsync(baggage);
        await _db.SaveChangesAsync();
    }
}
