using System.Collections.Generic;

namespace Flightmanagement.Models
{
    public class Aircraft
    {
        public int AircraftId { get; set; }
        public string TailNumber { get; set; } = "";
        public string Model { get; set; } = "";
        public int Capacity { get; set; }

        public ICollection<AircraftMaintenance> MaintenanceRecords { get; set; } = new List<AircraftMaintenance>();
    }
}
