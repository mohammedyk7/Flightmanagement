namespace Flightmanagement.DTOs
{
    public class ItinSegmentDto
    {
        public string FlightNumber { get; set; } = "";
        public DateTime Dep { get; set; }
        public DateTime Arr { get; set; }
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
    }

    public class PassengerItineraryDto
    {
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = "";
        public string BookingRef { get; set; } = "";
        public List<ItinSegmentDto> Segments { get; set; } = new();
        public int BookingId { get; set; }

    }
}
