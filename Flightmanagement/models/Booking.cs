using System;
using System.Collections.Generic;

namespace Flightmanagement.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string BookingRef { get; set; } = "";
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Confirmed";

        public int PassengerId { get; set; }
        public Passenger? Passenger { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
