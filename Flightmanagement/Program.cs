// Program.cs — Console app with DI + EF Core + CRUD + analytics menu
// .NET 8

using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Flightmanagement.Data;

// Interfaces
using Flightmanagement.Interfaces;

// Repositories
using Flightmanagement.Repositories;

// Services
using Flightmanagement.Services;

// Console CRUD
using Flightmanagement.Crud;

const bool ANALYTICS_ENABLED = true;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var cs = ctx.Configuration.GetConnectionString("Default")
                 ?? throw new InvalidOperationException("ConnectionStrings:Default missing");

        // DbContext
        services.AddDbContext<FlightContext>(opt => opt.UseSqlServer(cs));

        // ---------------- Repositories ----------------
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<ICrewRepository, CrewRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IAircraftRepository, AircraftRepository>();
        services.AddScoped<IBaggageRepository, BaggageRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();

        // ---------------- Services ----------------
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<IBookingService, BookingService>();

        // Analytics / Query service
        services.AddScoped<IFlightQueryService, FlightQueryService>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var sp = scope.ServiceProvider;

var db = sp.GetRequiredService<FlightContext>();
await db.Database.MigrateAsync();
Flightmanagement.Seed.DbSeeder.Seed(db);

// Debug info — verify the exact DB/Server you’re using
Console.WriteLine("Connected DB: " + db.Database.GetDbConnection().Database);
Console.WriteLine("Server:       " + db.Database.GetDbConnection().DataSource);
Console.WriteLine("Passengers:   " + await db.Passengers.CountAsync());

// Resolve services for menu actions
var flightSvc = sp.GetRequiredService<IFlightService>();
var bookingSvc = sp.GetRequiredService<IBookingService>();
var flightQuery = sp.GetRequiredService<IFlightQueryService>();

// ------------------------------ Loop ------------------------------
while (true)
{
    Console.WriteLine();
    PrintMenu(ANALYTICS_ENABLED);
    Console.Write("> ");

    var k = Console.ReadLine()?.Trim();

    try
    {
        switch (k)
        {
            case "1":
                await PassengerCrud.RunAsync(db);
                break;

            case "2":
                await CreateBookingAsync(bookingSvc);
                break;

            case "3":
                await CheckInTicketAsync(bookingSvc);
                break;

            case "4":
                await AddBaggageAsync(bookingSvc);
                break;

            case "5":
                await CancelBookingAsync(bookingSvc);
                break;

            case "6":
                await ListFlightsWindowAsync(flightSvc);
                break;

            case "7":
                await PrintDbStatsAsync(db);
                break;

            // -------------------- Analytics (only if enabled) --------------------
            case "8" when ANALYTICS_ENABLED:
                {
                    Console.Write("Date (UTC) yyyy-MM-dd: ");
                    var input = Console.ReadLine();

                    if (DateTime.TryParseExact(
                            input,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var manifestDate))
                    {
                        Console.WriteLine($"Date recorded successfully: {manifestDate:yyyy-MM-dd}");

                        var data = await flightQuery.GetDailyManifestAsync(manifestDate);
                        var list = data?.ToList() ?? new List<DailyManifestDto>();

                        if (list.Count == 0)
                        {
                            Console.WriteLine("No flights found for that date.");
                            break;
                        }

                        Console.WriteLine("\nFlight  Dep(UTC)           Arr(UTC)           Pax  Aircraft  Route");
                        Console.WriteLine(new string('-', 85));
                        foreach (var m in list)
                            Console.WriteLine($"{m.FlightNumber,-6} {m.DepartureUtc:yyyy-MM-dd HH:mm}  {m.ArrivalUtc:yyyy-MM-dd HH:mm}  {m.PassengerCount,3}  {m.AircraftTail,-8} {m.OriginIATA}->{m.DestIATA}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid date format.");
                    }
                    break;
                }

            case "9" when ANALYTICS_ENABLED: await RunTopRoutesRevenueAsync(flightQuery); break;
            case "10" when ANALYTICS_ENABLED: await RunOnTimePerformanceAsync(flightQuery); break;
            case "11" when ANALYTICS_ENABLED: await RunSeatOccupancyAsync(flightQuery); break;
            case "12" when ANALYTICS_ENABLED: await RunAvailableSeatsAsync(flightQuery); break;
            case "13" when ANALYTICS_ENABLED: await RunCrewConflictsAsync(flightQuery); break;
            case "14" when ANALYTICS_ENABLED: await RunPassengersWithConnectionsAsync(flightQuery); break;
            case "15" when ANALYTICS_ENABLED: await RunFrequentFliersAsync(flightQuery); break;
            case "16" when ANALYTICS_ENABLED: await RunMaintenanceAlertsAsync(flightQuery); break;
            case "17" when ANALYTICS_ENABLED: await RunBaggageOverweightAsync(flightQuery); break;
            case "18" when ANALYTICS_ENABLED: await RunSetPartitioningAsync(flightQuery); break;
            case "19" when ANALYTICS_ENABLED: await RunConversionOpsAsync(flightQuery); break;
            case "20" when ANALYTICS_ENABLED: await RunRunningTotalsAsync(flightQuery); break;

            case "21" when ANALYTICS_ENABLED:
                {
                    Console.Write("Forecast next N days (e.g., 7): ");
                    var days = ReadInt(min: 1);

                    Console.WriteLine($"Forecast period accepted: {days} day(s).");

                    var forecast = await flightQuery.ForecastNextNDaysAsync(days);
                    var list = forecast?.ToList() ?? new List<ForecastDto>();

                    // Fallback generator so user always sees data
                    if (list.Count == 0)
                    {
                        Console.WriteLine("No forecast data available from service — showing sample forecast.");
                        list = GenerateSampleForecast(days);
                    }

                    Console.WriteLine("\nDate         Forecast");
                    Console.WriteLine(new string('-', 28));

                    decimal total = 0m;
                    foreach (var f in list)
                    {
                        Console.WriteLine($"{f.Date:yyyy-MM-dd}  {f.Revenue,12:C}");
                        total += f.Revenue;
                    }

                    Console.WriteLine(new string('-', 28));
                    Console.WriteLine($"Total:      {total,12:C}");
                    Console.WriteLine($"Avg/day:    {(total / list.Count),12:C}");
                    break;
                }

            case "0":
                return;

            default:
                Console.WriteLine("Select a valid option.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ " + ex.GetBaseException().Message);
    }
}

// --------------------------- Menu printer -----------------------------
static void PrintMenu(bool showAnalytics)
{
    Console.WriteLine("=== Flight Management ===");
    Console.WriteLine("1)  Passenger CRUD");
    Console.WriteLine("2)  Create booking (with tickets)");
    Console.WriteLine("3)  Check-in ticket");
    Console.WriteLine("4)  Add baggage to ticket");
    Console.WriteLine("5)  Cancel booking");
    Console.WriteLine("6)  List flights in window");
    Console.WriteLine("7)  DB stats");

    if (showAnalytics)
    {
        Console.WriteLine("\n--- Analytics (LINQ 14 tasks) ---");
        Console.WriteLine("8)  Daily Flight Manifest");
        Console.WriteLine("9)  Top Routes by Revenue");
        Console.WriteLine("10) On-Time Performance");
        Console.WriteLine("11) Seat Occupancy Heatmap");
        Console.WriteLine("12) Find Available Seats (for a flight)");
        Console.WriteLine("13) Crew Scheduling Conflicts");
        Console.WriteLine("14) Passengers with Connections");
        Console.WriteLine("15) Frequent Fliers");
        Console.WriteLine("16) Maintenance Alert");
        Console.WriteLine("17) Baggage Overweight Alerts");
        Console.WriteLine("18) Complex Set/Partitioning (Union/Intersect/Except + Paging)");
        Console.WriteLine("19) Conversion Operators (ToDictionary/ToArray/AsEnumerable/OfType)");
        Console.WriteLine("20) Window-like Op (Running Totals)");
        Console.WriteLine("21) Forecasting (simple)");
    }

    Console.WriteLine("0)  Exit");
}

// --------------------------- CRUD Menu Handlers -----------------------------

static async Task CreateBookingAsync(IBookingService svc)
{
    Console.Write("Passenger Id: ");
    var passengerId = ReadInt();

    Console.Write("Number of tickets to add: ");
    var n = ReadInt(min: 1);

    var items = new List<(int flightId, string seat, decimal fare)>();
    for (int i = 0; i < n; i++)
    {
        Console.Write($" Flight Id [{i + 1}/{n}]: ");
        var flightId = ReadInt();

        Console.Write(" Seat (e.g., 12A): ");
        var seat = Console.ReadLine()!.Trim();

        Console.Write(" Fare: ");
        var fare = ReadDecimal();

        items.Add((flightId, seat, fare));
    }

    var booking = await svc.CreateBookingAsync(passengerId, items);
    Console.WriteLine($"✅ Booking created: #{booking.BookingId}  Ref: {booking.BookingRef}");
}

static async Task CheckInTicketAsync(IBookingService svc)
{
    Console.Write("Ticket Id: ");
    var id = ReadInt();
    await svc.CheckInAsync(id);
    Console.WriteLine("✅ Checked-in.");
}

static async Task AddBaggageAsync(IBookingService svc)
{
    Console.Write("Ticket Id: ");
    var id = ReadInt();

    Console.Write("Weight (kg): ");
    var kg = ReadDecimal();

    Console.Write("Tag number: ");
    var tag = Console.ReadLine()!.Trim();

    await svc.AddBaggageAsync(id, kg, tag);
    Console.WriteLine("✅ Baggage added.");
}

static async Task CancelBookingAsync(IBookingService svc)
{
    Console.Write("Booking Id: ");
    var id = ReadInt();
    await svc.CancelBookingAsync(id);
    Console.WriteLine("✅ Booking cancelled (tickets & baggage removed).");
}

static async Task ListFlightsWindowAsync(IFlightService svc)
{
    var from = AskDate("From (UTC) yyyy-MM-dd");
    var to = AskDate("To   (UTC) yyyy-MM-dd");

    var flights = await svc.GetFlightsInWindowAsync(from, to);
    if (flights.Count == 0)
    {
        Console.WriteLine("No flights in that window.");
        return;
    }

    Console.WriteLine("\nId   Flight   Dep(UTC)           Arr(UTC)           AvailSeats");
    Console.WriteLine(new string('-', 70));
    foreach (var f in flights)
    {
        var avail = await svc.GetAvailableSeatCountAsync(f.FlightId);
        Console.WriteLine(
            $"{f.FlightId,4} {f.FlightNumber,-7} {f.DepartureTime:yyyy-MM-dd HH:mm}  {f.ArrivalTime:yyyy-MM-dd HH:mm}  {avail,5}");
    }
}

// DB Stats
static async Task PrintDbStatsAsync(FlightContext db)
{
    Console.WriteLine("\n=== DB Stats ===");
    Console.WriteLine("Server:   " + db.Database.GetDbConnection().DataSource);
    Console.WriteLine("Database: " + db.Database.GetDbConnection().Database);

    var airports = await db.Airports.CountAsync();
    var aircraft = await db.Aircraft.CountAsync();
    var routes = await db.Routes.CountAsync();
    var flights = await db.Flights.CountAsync();
    var crew = await db.CrewMembers.CountAsync();
    var flightCrew = await db.FlightCrews.CountAsync();
    var passengers = await db.Passengers.CountAsync();
    var bookings = await db.Bookings.CountAsync();
    var tickets = await db.Tickets.CountAsync();
    var baggage = await db.Baggage.CountAsync();
    var maint = await db.Maintenance.CountAsync();

    Console.WriteLine($"Airports   : {airports}");
    Console.WriteLine($"Aircraft   : {aircraft}");
    Console.WriteLine($"Routes     : {routes}");
    Console.WriteLine($"Flights    : {flights}");
    Console.WriteLine($"Crew       : {crew}");
    Console.WriteLine($"FlightCrew : {flightCrew}");
    Console.WriteLine($"Passengers : {passengers}");
    Console.WriteLine($"Bookings   : {bookings}");
    Console.WriteLine($"Tickets    : {tickets}");
    Console.WriteLine($"Baggage    : {baggage}");
    Console.WriteLine($"Maintenance: {maint}");
}

// --------------------------- Analytics helpers -----------------------------

static async Task RunTopRoutesRevenueAsync(IFlightQueryService svc)
{
    var from = AskDate("From (UTC) yyyy-MM-dd");
    var to = AskDate("To   (UTC) yyyy-MM-dd");
    Console.Write("Top N: ");
    var topN = ReadInt(min: 1);

    var res = await svc.GetTopRoutesByRevenueAsync(from, to, topN);
    var list = res?.ToList() ?? new List<RouteRevenueDto>();
    if (list.Count == 0) { Console.WriteLine("No data."); return; }

    Console.WriteLine("\nRoute   Flights  Seats  AvgFare    Revenue");
    Console.WriteLine(new string('-', 55));
    foreach (var r in list)
        Console.WriteLine($"{r.RouteCode,-8} {r.FlightCount,7} {r.SeatsSold,6} {r.AvgFare,10:C} {r.TotalRevenue,10:C}");
}

static async Task RunOnTimePerformanceAsync(IFlightQueryService svc)
{
    var from = AskDate("From (UTC) yyyy-MM-dd");
    var to = AskDate("To   (UTC) yyyy-MM-dd");
    Console.Write("On-time threshold (minutes): ");
    var mins = ReadInt(min: 0);

    var res = await svc.GetOnTimePerformanceAsync(from, to, mins);
    var list = res?.ToList() ?? new List<OnTimeDto>();
    if (list.Count == 0) { Console.WriteLine("No data."); return; }

    Console.WriteLine("\nKey                   OnTime   Total   Pct");
    Console.WriteLine(new string('-', 50));
    foreach (var r in list)
        Console.WriteLine($"{r.Key,-20} {r.OnTimeCount,7} {r.TotalCount,7} {r.OnTimePct,6:P1}");
}

static async Task RunSeatOccupancyAsync(IFlightQueryService svc)
{
    var res = await svc.GetSeatOccupancyAsync();
    var list = res?.ToList() ?? new List<SeatOccDto>();
    if (list.Count == 0) { Console.WriteLine("No data."); return; }

    Console.WriteLine("\nFlight   Date        Occupancy  Route");
    Console.WriteLine(new string('-', 55));
    foreach (var r in list)
        Console.WriteLine($"{r.FlightNumber,-7} {r.DepartureDate:yyyy-MM-dd}  {r.OccupancyPct,9:P1}  {r.Origin}->{r.Destination}");
}

static async Task RunAvailableSeatsAsync(IFlightQueryService svc)
{
    Console.Write("Flight Id: ");
    var flightId = ReadInt(min: 1);
    var seats = await svc.GetAvailableSeatNumbersAsync(flightId);
    var list = seats?.ToList() ?? new List<string>();
    Console.WriteLine(list.Count == 0 ? "(none)" : string.Join(", ", list));
}

static async Task RunCrewConflictsAsync(IFlightQueryService svc)
{
    var res = await svc.GetCrewSchedulingConflictsAsync();
    var list = res?.ToList() ?? new List<CrewConflictDto>();
    if (list.Count == 0) { Console.WriteLine("No conflicts."); return; }

    Console.WriteLine("\nCrewId  Name                FlightA   FlightB   Detail");
    Console.WriteLine(new string('-', 70));
    foreach (var r in list)
        Console.WriteLine($"{r.CrewId,5}  {r.CrewName,-18} {r.FlightA,-8}  {r.FlightB,-8}  {r.ConflictDetail}");
}

static async Task RunPassengersWithConnectionsAsync(IFlightQueryService svc)
{
    Console.Write("Max layover hours (e.g., 6): ");
    var hours = ReadInt(min: 1);
    var res = await svc.GetPassengersWithConnectionsAsync(hours);
    var list = res?.ToList() ?? new List<ConnectionDto>();
    if (list.Count == 0) { Console.WriteLine("No connections found."); return; }
    foreach (var r in list)
        Console.WriteLine($"{r.PassengerName} | {r.Itinerary}");
}

static async Task RunFrequentFliersAsync(IFlightQueryService svc)
{
    Console.Write("Min flights to qualify (e.g., 5): ");
    var min = ReadInt(min: 1);
    var res = await svc.GetFrequentFliersAsync(min);
    var list = res?.ToList() ?? new List<FrequentFlierDto>();
    if (list.Count == 0) { Console.WriteLine("No frequent fliers."); return; }
    foreach (var r in list)
        Console.WriteLine($"{r.PassengerName} | Flights:{r.FlightCount} | Distance:{r.TotalDistance:N0} km");
}

static async Task RunMaintenanceAlertsAsync(IFlightQueryService svc)
{
    Console.Write("Alert horizon days (e.g., 14): ");
    var days = ReadInt(min: 0);
    var until = DateTime.UtcNow.Date.AddDays(days);
    var res = await svc.GetMaintenanceAlertsAsync(until);
    var list = res?.ToList() ?? new List<MaintenanceAlertDto>();
    if (list.Count == 0) { Console.WriteLine("No alerts."); return; }
    foreach (var r in list)
        Console.WriteLine($"{r.TailNumber} | Due:{r.DueDate:d} | Hours:{r.CumulativeHours:N1} | {r.Note}");
}

static async Task RunBaggageOverweightAsync(IFlightQueryService svc)
{
    Console.Write("Per-ticket weight limit (kg, default 30): ");
    var s = Console.ReadLine();
    decimal limit = 30m;
    _ = decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out limit);

    var res = await svc.GetBaggageOverweightAlertsAsync(limit);
    var list = res?.ToList() ?? new List<OverweightDto>();
    if (list.Count == 0) { Console.WriteLine("No overweight baggage."); return; }
    foreach (var r in list)
        Console.WriteLine($"Booking {r.BookingRef} | Ticket {r.TicketId} | Total {r.TotalWeightKg} kg");
}

static async Task RunSetPartitioningAsync(IFlightQueryService svc)
{
    var set = await svc.GetSetOperationSamplesAsync();
    Console.WriteLine("\nSet Operations:");
    Console.Write("Union     : "); foreach (var x in set.UnionSample) Console.Write(x + " "); Console.WriteLine();
    Console.Write("Intersect : "); foreach (var x in set.IntersectSample) Console.Write(x + " "); Console.WriteLine();
    Console.Write("Except    : "); foreach (var x in set.ExceptSample) Console.Write(x + " "); Console.WriteLine();

    Console.Write("\nPaging — page #: ");
    var page = ReadInt(min: 1);
    var pageRes = await svc.PageFlightsAsync(page, 10);
    var list = pageRes?.ToList() ?? new List<FlightPageDto>();
    if (list.Count == 0) { Console.WriteLine("No flights."); return; }
    Console.WriteLine("\nFlights:");
    foreach (var r in list)
        Console.WriteLine($"{r.FlightNumber} {r.DepartureDate:g} | {r.Origin}->{r.Destination}");
}

static async Task RunConversionOpsAsync(IFlightQueryService svc)
{
    var map = await svc.FlightsByNumberMapAsync();
    Console.WriteLine($"\nMap created with {map.Count} entries (ToDictionary). First 5 keys:");
    int shown = 0;
    foreach (var k in map.Keys)
    {
        Console.WriteLine(k);
        if (++shown == 5) break;
    }

    Console.Write("Top N routes for array demo: ");
    var top = ReadInt(min: 1);
    var arr = await svc.TopRoutesToArrayAsync(top);
    Console.WriteLine("Top routes array:");
    foreach (var r in arr) Console.WriteLine($"{r.RouteCode}");

    var asEnumCount = await svc.AsEnumerableSampleCountAsync();
    Console.WriteLine($"AsEnumerable sample count: {asEnumCount}");

    var ofType = await svc.OfTypeSampleAsync();
    Console.WriteLine($"OfType sample count: {ofType}");
}

static async Task RunRunningTotalsAsync(IFlightQueryService svc)
{
    Console.Write("How many days of history (e.g., 14): ");
    var days = ReadInt(min: 1);
    var running = await svc.GetRunningDailyRevenueAsync(days);
    var list = running?.ToList() ?? new List<RunningRevenueDto>();
    if (list.Count == 0) { Console.WriteLine("No data."); return; }

    Console.WriteLine("\nDate         Revenue      Running");
    Console.WriteLine(new string('-', 40));
    foreach (var r in list)
        Console.WriteLine($"{r.Date:yyyy-MM-dd}  {r.Revenue,10:C}  {r.RunningTotal,10:C}");
}

// --------------------------- Input Helpers -----------------------------

static int ReadInt(int? min = null, int? max = null)
{
    while (true)
    {
        var s = Console.ReadLine();
        if (int.TryParse(s, out var v))
        {
            if (min.HasValue && v < min.Value) { Console.Write($" ≥ {min}: "); continue; }
            if (max.HasValue && v > max.Value) { Console.Write($" ≤ {max}: "); continue; }
            return v;
        }
        Console.Write("Enter a number: ");
    }
}

static decimal ReadDecimal()
{
    while (true)
    {
        var s = Console.ReadLine();
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            return v;
        Console.Write("Enter a decimal number: ");
    }
}

static DateTime AskDate(string label)
{
    while (true)
    {
        Console.Write($"{label}: ");
        var s = Console.ReadLine();
        if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d))
            return d;
        Console.WriteLine("Use format yyyy-MM-dd (UTC).");
    }
}

// Fallback generator for Forecast (used if the service returns no data)
static List<ForecastDto> GenerateSampleForecast(int days)
{
    var start = DateTime.UtcNow.Date.AddDays(1);
    var baseRevenue = 12000m;      // tweak scale if you like
    var growthPerDay = 0.01m;      // 1% daily growth
    var list = new List<ForecastDto>(Math.Max(1, days));

    for (int i = 0; i < Math.Max(1, days); i++)
    {
        var date = start.AddDays(i);

        // weekly seasonality in [-10%, +10%]
        var weeklyAngle = (double)(i % 7) / 7.0 * 2.0 * Math.PI;
        var seasonality = (decimal)(Math.Sin(weeklyAngle) * 0.10);

        // small deterministic noise ~ [-2%, +2%]
        var rnd = new Random(HashCode.Combine(date.Year, date.Month, date.Day));
        var noise = (decimal)((rnd.NextDouble() - 0.5) * 0.04);

        var multiplier = 1m + (growthPerDay * i) + seasonality + noise;
        var revenue = decimal.Round(baseRevenue * multiplier, 2, MidpointRounding.AwayFromZero);
        if (revenue < 0m) revenue = baseRevenue * 0.5m;

        list.Add(new ForecastDto(date, revenue));
    }
    return list;
}
