using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Flightmanagement.Models
{
    public class CrewMember
    {
        [Key] public int CrewId { get; set; }
        public string FullName { get; set; } = "";
        public string Role { get; set; } = ""; // Pilot | CoPilot | FlightAttendant
        public string? LicenseNo { get; set; }

        public ICollection<FlightCrew> FlightCrews { get; set; } = new List<FlightCrew>();
    }
}
