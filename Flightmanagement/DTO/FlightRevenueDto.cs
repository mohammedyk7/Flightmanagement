namespace Flightmanagement.DTOs
{
    public class FlightRevenueDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = "";
        public int SeatsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgFare { get; set; }
    }
}
