using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;
using DTO = Flightmanagement.DTOs;
using FrequentFlierDto = Flightmanagement.DTOs.FrequentFlierDto;


// Keep your other aliases, but add this one:
using PrintDTO = Flightmanagement.Interfaces;   // the DTOs your console expects



namespace Flightmanagement.Services
{
    // Schema-agnostic stub: compiles even if your model names differ.
    public class FlightQueryService : IFlightQueryService
    {
        private readonly FlightContext _db; // kept for future real queries
        private const double AVG_SPEED_KPH = 750.0;   // simulate hours = km / speed
        private const double HOURS_THRESHOLD = 500.0; // alert if hours exceed this
        private const int MAX_DAYS_BETWEEN_MX = 90;   // alert if last maintenance older than this
        public FlightQueryService(FlightContext db) => _db = db;

        // 1) Daily Flight Manifest
        public Task<IReadOnlyList<DailyManifestDto>> GetDailyManifestAsync(DateTime date) =>
            Task.FromResult<IReadOnlyList<DailyManifestDto>>(Array.Empty<DailyManifestDto>());

        // 2) Top Routes by Revenue
        public async Task<IReadOnlyList<RouteRevenueDto>> GetTopRoutesByRevenueAsync(
          DateTime fromUtc, DateTime toUtc, int topN)
        {
            topN = Math.Max(1, topN);

            // ---- flights per route (done in SQL)
            var flightsByRoute = await _db.Flights
                .AsNoTracking()
                .Where(f => f.DepartureTime >= fromUtc && f.DepartureTime <= toUtc)
                .GroupBy(f => f.RouteId)
                .Select(g => new
                {
                    RouteId = g.Key,
                    FlightCount = g.Count()
                })
                .ToListAsync();

            // ---- ticket revenue per route (done in SQL)
            // Adjust if your model uses different names (e.g., Ticket.Price instead of Fare).
            var ticketAggByRoute = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.Flight.DepartureTime >= fromUtc && t.Flight.DepartureTime <= toUtc)
                .GroupBy(t => t.Flight.RouteId)
                .Select(g => new
                {
                    RouteId = g.Key,
                    SeatsSold = g.Count(),
                    AvgFare = g.Average(t => (decimal?)t.Fare) ?? 0m,
                    Revenue = g.Sum(t => (decimal?)t.Fare) ?? 0m
                })
                .ToListAsync();

            var revenueLookup = ticketAggByRoute.ToDictionary(x => x.RouteId);

            // ---- left-join in memory so routes with flights but zero tickets still appear
            var rows = flightsByRoute
                .Select(f =>
                {
                    revenueLookup.TryGetValue(f.RouteId, out var r);
                    return new RouteRevenueDto(
                        RouteCode: $"Route#{f.RouteId}",         // swap to real code if you have one
                        FlightCount: f.FlightCount,
                        SeatsSold: r?.SeatsSold ?? 0,
                        AvgFare: r?.AvgFare ?? 0m,
                        TotalRevenue: r?.Revenue ?? 0m
                    );
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ThenByDescending(x => x.FlightCount)
                .Take(topN)
                .ToList();

            return rows;
        }


        public async Task<IReadOnlyList<OnTimeDto>> GetOnTimePerformanceAsync(
          DateTime fromUtc, DateTime toUtc, int thresholdMinutes)
        {
            thresholdMinutes = Math.Max(0, thresholdMinutes);

            // Pull per-flight route + duration from SQL
            var flights = await _db.Flights
                .AsNoTracking()
                .Where(f => f.DepartureTime >= fromUtc && f.DepartureTime <= toUtc)
                .Select(f => new
                {
                    f.RouteId,
                    DurationMin = EF.Functions.DateDiffMinute(f.DepartureTime, f.ArrivalTime)
                })
                .ToListAsync();

            // Group in memory by route, compare each duration to the route's average
            var rows = flights
                .GroupBy(x => x.RouteId)
                .Select(g =>
                {
                    var total = g.Count();
                    if (total == 0) return new OnTimeDto($"Route#{g.Key}", 0, 0, 0m);

                    var avg = g.Average(v => v.DurationMin); // double
                    var onTime = g.Count(v => Math.Abs(v.DurationMin - avg) <= thresholdMinutes);

                    return new OnTimeDto(
                        Key: $"Route#{g.Key}",
                        OnTimeCount: onTime,
                        TotalCount: total,
                        OnTimePct: (decimal)onTime / total
                    );
                })
                .OrderByDescending(r => r.OnTimePct)
                .ThenByDescending(r => r.TotalCount)
                .ToList();

            return rows;
        }

        // 4) Seat Occupancy Heatmap
        public async Task<IReadOnlyList<SeatOccDto>> GetSeatOccupancyAsync()
        {
            // Window to inspect (adjust as needed)
            var fromUtc = DateTime.UtcNow.Date.AddDays(-60);
            var toUtc = DateTime.UtcNow.Date.AddDays(1);

            // Seats sold per flight (SQL)
            var ticketsByFlight = _db.Tickets
                .AsNoTracking()
                .Where(t => t.Flight.DepartureTime >= fromUtc && t.Flight.DepartureTime < toUtc)
                .GroupBy(t => t.FlightId)
                .Select(g => new { FlightId = g.Key, SeatsSold = g.Count() });

            // Join Flights + Aircraft (for capacity) + left join ticket counts
            var raw = await (
                from f in _db.Flights.AsNoTracking()
                where f.DepartureTime >= fromUtc && f.DepartureTime < toUtc
                join a in _db.Aircraft.AsNoTracking() on f.AircraftId equals a.AircraftId
                join tb in ticketsByFlight on f.FlightId equals tb.FlightId into tb0
                from tb in tb0.DefaultIfEmpty()
                select new
                {
                    f.FlightId,
                    f.FlightNumber,
                    f.DepartureTime,
                    f.RouteId,                                // we’ll label as "Route#<id>"
                    Capacity = (int?)a.Capacity ?? 0,
                    SeatsSold = (int?)(tb.SeatsSold) ?? 0
                })
                .ToListAsync();

            // Project and rank
            var list = raw
                .Select(x =>
                {
                    var pct = x.Capacity > 0 ? (decimal)x.SeatsSold / x.Capacity : 0m;
                    return new SeatOccDto(
                        FlightNumber: x.FlightNumber,
                        DepartureDate: x.DepartureTime.Date,
                        OccupancyPct: pct,
                        // Reuse existing DTO fields to show a single label:
                        Origin: $"Route#{x.RouteId}",
                        Destination: ""                       // keeps your "Route" column working
                    );
                })
                .OrderByDescending(d => d.OccupancyPct)
                .ThenBy(d => d.DepartureDate)
                .Take(20)
                .ToList();

            return list;
        }


        // 5) Find Available Seats (for a flight)
        public async Task<IReadOnlyList<string>> GetAvailableSeatNumbersAsync(int flightId)
        {
            // 1) Capacity for this flight via Aircraft
            var capacity = await (
                from f in _db.Flights.AsNoTracking()
                join a in _db.Aircraft.AsNoTracking() on f.AircraftId equals a.AircraftId
                where f.FlightId == flightId
                select a.Capacity
            ).FirstOrDefaultAsync();

            if (capacity <= 0) return Array.Empty<string>();

            // 2) Full seat map derived from capacity (A..F per row)
            var allSeats = BuildSeatMap(capacity).ToList();

            // 3) Booked seats for this flight (adjust property name if not "Seat")
            var booked = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.FlightId == flightId /* && !t.IsCancelled */)
                .Select(t => t.SeatNumber)
                
                .Where(s => s != null && s != "")
                .ToListAsync();

            var bookedSet = new HashSet<string>(booked, StringComparer.OrdinalIgnoreCase);

            // 4) Available = All \ Booked, ordered naturally (row then letter)
            var available = allSeats
                .Where(s => !bookedSet.Contains(s))
                .OrderBy(SeatRowThenLetterKey)
                .ToList();

            return available;
        }

        // ---------- helpers (keep them private in the service) ----------

        // Build 1A,1B,... based on capacity; default layout: 6 across (A..F)
        private static IEnumerable<string> BuildSeatMap(int capacity, int seatsPerRow = 6)
        {
            const string letters = "ABCDEF";
            if (seatsPerRow < 1 || seatsPerRow > letters.Length) seatsPerRow = 6;

            int rows = (int)Math.Ceiling(capacity / (double)seatsPerRow);
            int count = 0;

            for (int row = 1; row <= rows && count < capacity; row++)
            {
                for (int i = 0; i < seatsPerRow && count < capacity; i++)
                {
                    yield return row.ToString(CultureInfo.InvariantCulture) + letters[i];
                    count++;
                }
            }
        }

        // Sort key: (row number, seat letter)
        private static (int row, char seat) SeatRowThenLetterKey(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return (int.MaxValue, '\uffff');

            int i = 0;
            while (i < code.Length && char.IsDigit(code[i])) i++;

            int row = 0;
            if (i > 0) int.TryParse(code.Substring(0, i), out row);

            char seat = (i < code.Length) ? char.ToUpperInvariant(code[i]) : 'Z';
            return (row, seat);
        }


        // 6) Crew Scheduling Conflicts
        public async Task<IReadOnlyList<CrewConflictDto>> GetCrewSchedulingConflictsAsync()
        {
            // 1) Find overlapping flight pairs per crew (SQL only)
            var pairs = await (
                from fc1 in _db.FlightCrews.AsNoTracking()
                join fc2 in _db.FlightCrews.AsNoTracking()
                    on fc1.CrewId equals fc2.CrewId
                where fc1.FlightId < fc2.FlightId

                join f1 in _db.Flights.AsNoTracking() on fc1.FlightId equals f1.FlightId
                join f2 in _db.Flights.AsNoTracking() on fc2.FlightId equals f2.FlightId

                // overlap: A starts before B ends AND B starts before A ends
                where f1.DepartureTime < f2.ArrivalTime
                   && f2.DepartureTime < f1.ArrivalTime

                select new
                {
                    fc1.CrewId,
                    FlightA = f1.FlightNumber,
                    FlightB = f2.FlightNumber,
                    A_Dep = f1.DepartureTime,
                    A_Arr = f1.ArrivalTime,
                    B_Dep = f2.DepartureTime,
                    B_Arr = f2.ArrivalTime
                }
            ).ToListAsync();

            if (pairs.Count == 0) return Array.Empty<CrewConflictDto>();

            // 2) Load the crew rows for those CrewIds (SQL)
            var crewIds = pairs.Select(p => p.CrewId).Distinct().ToList();
            var crewRows = await _db.CrewMembers
                .AsNoTracking()
                .Where(c => crewIds.Contains(c.CrewId))
                .ToListAsync();

            // 3) Build a name dictionary in memory (reflection: try common property names)
            var nameMap = new Dictionary<int, string>();
            foreach (var c in crewRows)
            {
                nameMap[c.CrewId] = BuildCrewName(c);
            }

            // 4) Project to DTOs (in memory)
            var result = pairs
                .Select(p => new CrewConflictDto(
                    CrewId: p.CrewId,
                    CrewName: nameMap.TryGetValue(p.CrewId, out var n) ? n : $"Crew#{p.CrewId}",
                    FlightA: p.FlightA,
                    FlightB: p.FlightB,
                    ConflictDetail:
                        $"[{p.A_Dep:yyyy-MM-dd HH:mm}–{p.A_Arr:HH:mm}] overlaps [{p.B_Dep:yyyy-MM-dd HH:mm}–{p.B_Arr:HH:mm}]"
                ))
                .OrderBy(r => r.CrewId)
                .ThenBy(r => r.FlightA)
                .ThenBy(r => r.FlightB)
                .ToList();

            return result;
        }

        // ----- helpers -----

        private static string BuildCrewName(object crew)
        {
            var t = crew.GetType();

            string? GetStr(params string[] candidates)
            {
                foreach (var n in candidates)
                {
                    var pi = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (pi != null && pi.PropertyType == typeof(string))
                    {
                        var val = pi.GetValue(crew) as string;
                        if (!string.IsNullOrWhiteSpace(val)) return val!;
                    }
                }
                return null;
            }

            // Try single-field “full name”
            var full = GetStr("Name", "FullName", "CrewName", "DisplayName");
            if (!string.IsNullOrWhiteSpace(full)) return full!;

            // Try First+Last
            var first = GetStr("FirstName", "GivenName");
            var last = GetStr("LastName", "Surname", "FamilyName");
            var composed = $"{first} {last}".Trim();
            if (!string.IsNullOrWhiteSpace(composed)) return composed;

            // Fallback to CrewId
            var idPi = t.GetProperty("CrewId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var id = idPi?.GetValue(crew)?.ToString() ?? "?";
            return $"Crew#{id}";
        }

        // 7) Passengers with Connections
        public async Task<IReadOnlyList<ConnectionDto>> GetPassengersWithConnectionsAsync(int maxLayoverHours)
        {
            maxLayoverHours = Math.Max(1, maxLayoverHours);

            var fromUtc = DateTime.UtcNow.AddDays(-60);
            var toUtc = DateTime.UtcNow.AddDays(1);

            // Build flight segments per booking (Ticket -> Booking -> Passenger -> Flight)
            var segments = await
                (from t in _db.Tickets.AsNoTracking()
                 join b in _db.Bookings.AsNoTracking() on t.BookingId equals b.BookingId
                 join p in _db.Passengers.AsNoTracking() on b.PassengerId equals p.PassengerId
                 join f in _db.Flights.AsNoTracking() on t.FlightId equals f.FlightId
                 where f.DepartureTime >= fromUtc && f.DepartureTime <= toUtc
                 select new
                 {
                     b.BookingId,
                     // adjust if the property is different in your model (e.g., p.Name or p.FullName)
                     PassengerName = (p.FullName ),
                     f.FlightNumber,
                     f.ArrivalTime,
                     f.DepartureTime,
                     f.RouteId
                 })
                .ToListAsync();

            // Group by booking+passenger and order segments by time
            var perBooking = segments
                .GroupBy(x => new { x.BookingId, x.PassengerName })
                .Select(g => new
                {
                    g.Key.PassengerName,
                    Segs = g.OrderBy(s => s.DepartureTime).ToList()
                })
                .ToList();

            var results = new List<ConnectionDto>();
            var maxLayoverMinutes = maxLayoverHours * 60;

            foreach (var b in perBooking)
            {
                for (int i = 0; i < b.Segs.Count - 1; i++)
                {
                    var a = b.Segs[i];
                    var c = b.Segs[i + 1];

                    var layover = (int)(c.DepartureTime - a.ArrivalTime).TotalMinutes;
                    if (layover < 0 || layover > maxLayoverMinutes) continue;

                    results.Add(new ConnectionDto(
     b.PassengerName,
     $"{a.FlightNumber} ({a.ArrivalTime:yyyy-MM-dd HH:mm})  →  " +
     $"{c.FlightNumber} ({c.DepartureTime:yyyy-MM-dd HH:mm})  " +
     $"[Route#{a.RouteId}→Route#{c.RouteId}, Layover {layover} min]"
 ));

                }
            }

            return results;
        }



        // 8) Frequent Fliers
        // using Microsoft.EntityFrameworkCore;
        // using Flightmanagement.DTOs;

        public async Task<IReadOnlyList<FrequentFlierDto>> GetFrequentFliersAsync(int minFlights)
        {
            minFlights = Math.Max(1, minFlights);

            var q =
                from t in _db.Tickets.AsNoTracking()
                join b in _db.Bookings.AsNoTracking() on t.BookingId equals b.BookingId
                join p in _db.Passengers.AsNoTracking() on b.PassengerId equals p.PassengerId
                join f in _db.Flights.AsNoTracking() on t.FlightId equals f.FlightId
                join r in _db.Routes.AsNoTracking() on f.RouteId equals r.RouteId
                group r by new { p.PassengerId, p.FullName } into g
                where g.Count() >= minFlights
                select new
                {
                    PassengerId = g.Key.PassengerId,
                    PassengerName = g.Key.FullName,
                    FlightCount = g.Count(),
                    // cast to nullable to avoid NULL issues on SUM in SQL
                    TotalDistanceKm = g.Sum(x => (int?)x.DistanceKm) ?? 0
                };

            var rows = await q
                .OrderByDescending(x => x.FlightCount)
                .ThenByDescending(x => x.TotalDistanceKm)
                .ThenBy(x => x.PassengerName)
                .Select(x => new FrequentFlierDto
                {
                    PassengerId = x.PassengerId,
                    PassengerName = x.PassengerName ?? string.Empty,
                    FlightCount = x.FlightCount,
                    TotalDistanceKm = x.TotalDistanceKm
                })
                .ToListAsync();

            return rows;
        }





        // 9) Maintenance Alert
        public async Task<IReadOnlyList<MaintenanceAlertDto>> GetMaintenanceAlertsAsync(DateTime until)
        {
            // --- 1) Sum distance per aircraft (compute hours)
            var distPerAircraft = await (
                from f in _db.Flights.AsNoTracking()
                join r in _db.Routes.AsNoTracking() on f.RouteId equals r.RouteId
                where f.DepartureTime <= until
                group r by f.AircraftId into g
                select new
                {
                    AircraftId = g.Key,
                    TotalKm = g.Sum(x => (int?)x.DistanceKm) ?? 0
                }
            ).ToListAsync();

            if (distPerAircraft.Count == 0)
                return Array.Empty<MaintenanceAlertDto>();

            var aircraftIds = distPerAircraft.Select(x => x.AircraftId).Distinct().ToList();

            // --- 2) Tail numbers (this part compiles against your real columns)
            var tails = await _db.Aircraft
                .AsNoTracking()
                .Where(a => aircraftIds.Contains(a.AircraftId))
                .Select(a => new { a.AircraftId, a.TailNumber })
                .ToListAsync();
            var tailById = tails.ToDictionary(x => x.AircraftId, x => x.TailNumber ?? $"Aircraft#{x.AircraftId}");

            // --- 3) Latest maintenance date per aircraft, but WITHOUT knowing column/DbSet names
            // Use the generic set and resolve column names by reflection.
            var maintRows = await _db.Set<AircraftMaintenance>()     // no need for _db.AircraftMaintenances
                .AsNoTracking()
                .ToListAsync();

            DateTime? GetDate(object o) => GetNullableDate(o,
                "MaintenanceDate", "Date", "PerformedOn", "ServiceDate", "LastService", "MaintainedAt");

            int? GetAId(object o) => GetInt(o, "AircraftId", "AircraftID", "Aircraft_Id", "AircraftRefId");

            var lastMxById = maintRows
                .Select(m => new { Id = GetAId(m), Dt = GetDate(m) })
                .Where(x => x.Id.HasValue && x.Dt.HasValue && x.Dt!.Value <= until)
                .GroupBy(x => x.Id!.Value)
                .ToDictionary(g => g.Key, g => g.Max(v => v.Dt));

            // --- 4) Build alerts
            var results = new List<MaintenanceAlertDto>();

            foreach (var x in distPerAircraft)
            {
                var tail = tailById.TryGetValue(x.AircraftId, out var t) ? t : $"Aircraft#{x.AircraftId}";
                lastMxById.TryGetValue(x.AircraftId, out var last);

                var hours = x.TotalKm / AVG_SPEED_KPH;
                var byHours = hours >= HOURS_THRESHOLD;

                var byDate = last is DateTime lm
                    ? lm.AddDays(MAX_DAYS_BETWEEN_MX) <= until
                    : true; // unknown/never maintained → due

                if (!byHours && !byDate) continue;

                var note = (byHours, byDate) switch
                {
                    (true, true) => "Hours & LastMaintenance",
                    (true, false) => "Hours",
                    (false, true) => "LastMaintenance",
                    _ => ""
                };

                var due = last is DateTime lm2 ? lm2.AddDays(MAX_DAYS_BETWEEN_MX) : until;

                // positional record ctor: (TailNumber, DueDate, CumulativeHours, Note)
                results.Add(new MaintenanceAlertDto(
                    tail,
                    due,
                    Math.Round(hours, 2),
                    note
                ));
            }

            return results
                .OrderBy(r => r.DueDate)
                .ThenByDescending(r => r.CumulativeHours)
                .ToList();
        }

        /* ---------- tiny reflection helpers (keep once in class) ---------- */
        private static string? GetString(object o, params string[] names)
        {
            var t = o.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p?.PropertyType == typeof(string)) return (string?)p.GetValue(o);
            }
            return null;
        }
        private static int? GetInt(object o, params string[] names)
        {
            var t = o.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;
                var v = p.GetValue(o);
                if (v is int i) return i;
                if (v is int ni) return ni;
            }
            return null;
        }
        private static DateTime? GetNullableDate(object o, params string[] names)
        {
            var t = o.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;
                var v = p.GetValue(o);
                if (v is DateTime d) return d;
                if (v is DateTime nd) return nd;
            }
            return null;
        }


        // 10) Baggage Overweight Alerts
        public async Task<IReadOnlyList<DTO.OverweightDto>> GetBaggageOverweightAlertsAsync(decimal perTicketLimitKg)
        {
            if (perTicketLimitKg <= 0) perTicketLimitKg = 30m;

            // 1) Aggregate baggage weights per ticket (EF-friendly)
            var totalsPerTicket =
                _db.Baggage
                   .GroupBy(b => b.TicketId)
                   .Select(g => new
                   {
                       TicketId = g.Key,
                       // If WeightKg is nullable use the nullable Sum + coalesce:
                       Total = g.Sum(x => (decimal?)x.WeightKg) ?? 0m
                       // If it's non-nullable, this also works: Total = g.Sum(x => x.WeightKg)
                   });

            // 2) Join to tickets and bookings, then filter by limit
            var query =
                from t in _db.Tickets
                join gt in totalsPerTicket on t.TicketId equals gt.TicketId
                join bk in _db.Bookings on t.BookingId equals bk.BookingId
                where gt.Total > perTicketLimitKg
                select new DTO.OverweightDto
                {
                    TicketId = t.TicketId,
                    BookingRef = bk.BookingRef ?? bk.BookingId.ToString(),
                    TotalWeightKg = Math.Round(gt.Total, 2)
                };

            return await query
                .AsNoTracking()
                .OrderByDescending(x => x.TotalWeightKg)
                .ToListAsync();
        }


        // 11) Complex Set/Partitioning Examples
        public async Task<SetOpsDto> GetSetOperationSamplesAsync()
        {
            // 1) Frequent fliers: passengers with >= 2 bookings  (subquery → IN)
            var frequentIds =
                _db.Bookings
                   .GroupBy(b => b.PassengerId)
                   .Where(g => g.Count() >= 2)
                   .Select(g => g.Key);

            var frequent = await _db.Passengers
                .Where(p => frequentIds.Contains(p.PassengerId))
                .Select(p => p.FullName)                // change to your name field if different
                .AsNoTracking()
                .ToListAsync();

            // 2) VIPs: top “spenders” (fallback = most tickets to avoid Price column issues)
            var vipIds =
                _db.Tickets
                   .Join(_db.Bookings, t => t.BookingId, b => b.BookingId, (t, b) => new { b.PassengerId })
                   .GroupBy(x => x.PassengerId)
                   .OrderByDescending(g => g.Count())
                   .Select(g => g.Key)
                   .Take(200);

            var vip = await _db.Passengers
                .Where(p => vipIds.Contains(p.PassengerId))
                .Select(p => p.FullName)
                .AsNoTracking()
                .ToListAsync();

            // ---------- Canceled passengers (optional & safe) ----------
            var bookingEntity = _db.Model.FindEntityType(typeof(Flightmanagement.Models.Booking));
            bool hasIsCanceled = bookingEntity?.FindProperty("IsCanceled") != null;
            bool hasStatus = bookingEntity?.FindProperty("Status") != null;

            IQueryable<string> canceledQ = Enumerable.Empty<string>().AsQueryable();

            if (hasIsCanceled)
            {
                canceledQ =
                    from b in _db.Bookings
                    where EF.Property<bool>(b, "IsCanceled") == true
                    join p in _db.Passengers on b.PassengerId equals p.PassengerId
                    select p.FullName;
            }
            else if (hasStatus)
            {
                canceledQ =
                    from b in _db.Bookings
                    where EF.Property<string>(b, "Status") == "Canceled"
                    join p in _db.Passengers on b.PassengerId equals p.PassengerId
                    select p.FullName;
            }

            // If neither property exists, canceled stays empty (no filter applied)
            var canceled = await canceledQ
                .AsNoTracking()
                .Distinct()
                .ToListAsync();


            // 4) Set ops (in-memory)
            var union = vip.Union(frequent).Distinct().OrderBy(x => x).ToList();
            var intersect = vip.Intersect(frequent).Distinct().OrderBy(x => x).ToList();
            var except = union.Except(canceled).Distinct().OrderBy(x => x).ToList();

            return new SetOpsDto(union, intersect, except);
        }


        // 11b) Paging demo
        public async Task<IReadOnlyList<DTO.FlightPageDto>> PageFlightsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var skip = (page - 1) * pageSize;

            // <<< EDIT THESE 3 NAMES IF NEEDED >>>
            const string DepartureProp = "DepartureTime";   // e.g. "DepartureDate" or "DepartureUtc"
            const string RouteOriginProp = "OriginIATA";      // e.g. "FromIATA", "FromCode", "From"
            const string RouteDestProp = "DestIATA";        // e.g. "ToIATA", "ToCode", "To"

            var query =
                from f in _db.Flights
                join r in _db.Routes on f.RouteId equals r.RouteId
                orderby EF.Property<DateTime>(f, DepartureProp)
                select new DTO.FlightPageDto
                {
                    FlightNumber = EF.Property<string>(f, "FlightNumber"),
                    DepartureDate = EF.Property<DateTime>(f, DepartureProp),
                    // Use EF.Property so we don’t require r.Origin/r.Destination to exist
                    Origin = EF.Property<string>(r, RouteOriginProp),
                    Destination = EF.Property<string>(r, RouteDestProp)
                };

            return await query
                .Skip(skip)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }


        // 12) Conversion Operators
        
        public async Task<Dictionary<string, int>> FlightsByNumberMapAsync()
        {
            // ToDictionary: key = FlightNumber, value = 0 (demo only)
            var keys = await _db.Flights
                .Select(f => f.FlightNumber)
                .Where(fn => fn != null && fn != "")
                .Distinct()
                .ToListAsync();

            return keys.ToDictionary(k => k!, _ => 0);
        }


        public async Task<PrintDTO.RouteRevenueDto[]> TopRoutesToArrayAsync(int topN)
        {
            var grouped = await _db.Flights
                .Select(f => f.FlightNumber)
                .Where(fn => !string.IsNullOrWhiteSpace(fn))
                .GroupBy(fn => fn!)
                .Select(g => new { RouteCode = g.Key, FlightCount = g.Count() })
                .OrderByDescending(x => x.FlightCount)
                .Take(Math.Max(1, topN))
                .ToListAsync();

            return grouped.Select(x => new PrintDTO.RouteRevenueDto(
                x.RouteCode,     // string RouteCode
                x.FlightCount,   // int    FlightCount
                0,               // int    SeatsSold  (demo)
                0m,              // decimal AvgFare   (demo)
                0m               // decimal TotalRevenue (demo)
            ))
            .ToArray();
        }
        public Task<int> AsEnumerableSampleCountAsync()
        {
            // AsEnumerable: force in-memory and do a simple count
            var count = _db.Flights.AsEnumerable().Count();
            return Task.FromResult(count);
        }

        public Task<int> OfTypeSampleAsync()
        {
            // OfType<T> demo on a mixed collection
            object[] mixed = { "A", 1, "B", 2.5, 3, "C", 4L, 5m };
            var onlyInts = mixed.OfType<int>().Count();
            return Task.FromResult(onlyInts);
        }
        // 13) Window-like Operation (running totals)
        public async Task<IReadOnlyList<RunningRevenueDto>> GetRunningDailyRevenueAsync(int days)
        {
            days = Math.Max(1, days);
            var to = DateTime.UtcNow.Date;
            var from = to.AddDays(-(days - 1));

            // 1) Daily revenue: sum ticket fares per flight departure date
            var daily = await _db.Tickets
                .Join(_db.Flights,
                      t => t.FlightId,
                      f => f.FlightId,
                      (t, f) => new { f.DepartureTime, t.Fare })
                .Where(x => x.DepartureTime.Date >= from && x.DepartureTime.Date <= to)
                .GroupBy(x => x.DepartureTime.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => (decimal?)x.Fare) ?? 0m
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // 2) Running total (window-like accumulation)
            decimal running = 0m;
            var result = new List<RunningRevenueDto>(daily.Count);
            foreach (var d in daily)
            {
                running += d.Revenue;
                result.Add(new RunningRevenueDto(d.Date, d.Revenue, running));
            }

            return result;
        }
        // 14) Forecasting (simple)
        public Task<IReadOnlyList<ForecastDto>> ForecastNextNDaysAsync(int days) =>
            Task.FromResult<IReadOnlyList<ForecastDto>>(Array.Empty<ForecastDto>());
    }
}
