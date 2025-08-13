// Models/Airport.cs
using Flightmanagement.Models;

public class Airport
{
    public int AirportId { get; set; }
    public string IATA { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string TimeZone { get; set; } = "";

    // Inverse navs (optional, but recommended to be explicit)
    public ICollection<Route> OriginRoutes { get; set; } = new List<Route>();
    public ICollection<Route> DestinationRoutes { get; set; } = new List<Route>();
}
