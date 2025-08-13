using Microsoft.EntityFrameworkCore; // Needed for [Precision]

namespace Flightmanagement.Models
{
    public class Baggage
    {
        public int BaggageId { get; set; }

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        [Precision(18, 2)] // Fixes EF Core warning by setting precision and scale
        public decimal WeightKg { get; set; }

        public string TagNumber { get; set; } = "";
    }
}
