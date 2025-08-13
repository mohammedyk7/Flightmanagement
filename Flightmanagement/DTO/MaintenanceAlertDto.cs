namespace Flightmanagement.DTOs
{
    public class MaintenanceAlertDto
    {
        public int AircraftId { get; set; }
        public string TailNumber { get; set; } = "";
        public int TotalDistanceKm { get; set; }
        public double EstimatedHours { get; set; }
        public DateTime? LastMaintenance { get; set; }
        public string Reason { get; set; } = ""; // "Hours" or "LastMaintenance"
    }
}
