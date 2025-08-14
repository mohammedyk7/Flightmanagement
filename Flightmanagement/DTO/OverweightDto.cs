namespace Flightmanagement.DTOs
{
    public class OverweightDto
    {
        public int TicketId { get; set; }
        public string BookingRef { get; set; } = "";
        public decimal TotalWeightKg { get; set; }
    }
}
