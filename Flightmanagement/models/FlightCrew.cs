using Microsoft.EntityFrameworkCore;

namespace Flightmanagement.Models
{
    // 👇 tell EF the composite PK
    [PrimaryKey(nameof(FlightId), nameof(CrewId))]
    public class FlightCrew
    {
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        public int CrewId { get; set; }            // <-- must match the PK name on CrewMember
        public CrewMember? Crew { get; set; }

        public string RoleOnFlight { get; set; } = "";
    }
}
