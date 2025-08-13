// Models/Route.cs
public class Route
{
    public int RouteId { get; set; }

    // explicit FKs
    public int OriginAirportId { get; set; }
    public int DestinationAirportId { get; set; }

    // navs
    public Airport OriginAirport { get; set; } = default!;
    public Airport DestinationAirport { get; set; } = default!;

    public int DistanceKm { get; set; }
}
