using System.Collections.Generic;
using Microsoft.EntityFrameworkCore; // Needed for [Precision]

namespace Flightmanagement.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        public string SeatNumber { get; set; } = "";

        [Precision(18, 2)] // Sets SQL precision and scale for decimal column
        public decimal Fare { get; set; }

        public bool CheckedIn { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public ICollection<Baggage> Baggage { get; set; } = new List<Baggage>();
    }
}
