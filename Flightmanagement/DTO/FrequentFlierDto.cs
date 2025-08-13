namespace Flightmanagement.DTOs
{
    public class FrequentFlierDto
    {
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = "";
        public int FlightsCount { get; set; }
        public int TotalDistanceKm { get; set; }
    }
}
