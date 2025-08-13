namespace Flightmanagement.DTOs
{
    public class FlightManifestDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AircraftTail { get; set; } = "";
        public int PassengerCount { get; set; }
        public decimal TotalBaggageKg { get; set; }
    }
}
