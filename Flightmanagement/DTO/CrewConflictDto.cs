namespace Flightmanagement.DTOs
{
    public class CrewConflictDto
    {
        public int CrewId { get; set; }
        public string CrewName { get; set; } = "";
        public int FlightAId { get; set; }
        public int FlightBId { get; set; }
        public DateTime FlightADep { get; set; }
        public DateTime FlightBDep { get; set; }
    }
}
