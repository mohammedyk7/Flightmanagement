namespace Flightmanagement.DTOs
{
    public class FlightsPerDayDto
    {
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = "";
        public int FlightCount { get; set; }          // pick ONE name
        public int TotalDistanceKm { get; set; }      // keep Km if you like
    }
}
