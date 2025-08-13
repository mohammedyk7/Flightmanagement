namespace Flightmanagement.DTOs
{
    public class SeatOccupancyDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public int Capacity { get; set; }
        public int SeatsSold { get; set; }
        public decimal Occupancy => Capacity == 0 ? 0 : Math.Round((decimal)SeatsSold / Capacity, 3);
    }
}
