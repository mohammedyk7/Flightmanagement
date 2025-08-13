namespace Flightmanagement.DTOs
{
    public class OnTimePerformanceDto
    {
        public string Route { get; set; } = "";
        public int Flights { get; set; }
        public int OnTime { get; set; }
        public decimal PctOnTime => Flights == 0 ? 0 : Math.Round((decimal)OnTime / Flights, 3);
    }
}
