namespace Flightmanagement.DTOs
{
    public class RouteRevenueDto
    {
        public int RouteId { get; set; }
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public int SeatsSold { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgFare { get; set; }
    }
}
