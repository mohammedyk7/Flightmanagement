using System;
using System.ComponentModel.DataAnnotations;

namespace Flightmanagement.Models
{
    public class AircraftMaintenance
    {
        [Key] public int MaintenanceId { get; set; }

        public int AircraftId { get; set; }
        public Aircraft? Aircraft { get; set; }

        public DateTime MaintenanceDate { get; set; }
        public string Type { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
