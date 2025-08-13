namespace Flightmanagement.DTOs
{
    public class FlightDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AircraftTail { get; set; } = "";
        public int AircraftCapacity { get; set; }
    }
}
