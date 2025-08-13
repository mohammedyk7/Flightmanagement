using Flightmanagement.Models;

public class Flight
{
    public int FlightId { get; set; }
    public string FlightNumber { get; set; } = string.Empty;

    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }

    public string Status { get; set; } = "Scheduled";

    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;

    public int AircraftId { get; set; }
    public Aircraft Aircraft { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    // ✅ ADD THIS:
    public ICollection<FlightCrew> FlightCrews { get; set; } = new List<FlightCrew>();
}
