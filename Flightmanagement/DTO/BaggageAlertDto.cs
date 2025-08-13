namespace Flightmanagement.DTOs
{
    public class BaggageAlertDto
    {
        public int TicketId { get; set; }
        public string PassengerName { get; set; } = "";
        public string FlightNumber { get; set; } = "";
        public decimal TotalWeightKg { get; set; }
    }
}
