using System;
using System.Collections.Generic;
using System.Linq;
using Flightmanagement.Data;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;

namespace Flightmanagement.Seed
{
    public static class DbSeeder
    {
        // Tops up to the required minimum counts
        public static void Seed(FlightContext db)
        {
            var rnd = new Random(123);

            // ---------------- Airports (>=10) ----------------
            if (db.Airports.Count() < 10)
            {
                var baseAirports = new[]
                {
                    new Airport{ IATA="MCT", Name="Muscat Intl",  City="Muscat",      Country="Oman",          TimeZone="Asia/Muscat" },
                    new Airport{ IATA="DXB", Name="Dubai Intl",   City="Dubai",       Country="UAE",           TimeZone="Asia/Dubai" },
                    new Airport{ IATA="DOH", Name="Hamad Intl",   City="Doha",        Country="Qatar",         TimeZone="Asia/Qatar" },
                    new Airport{ IATA="BAH", Name="Bahrain Intl", City="Manama",      Country="Bahrain",       TimeZone="Asia/Bahrain" },
                    new Airport{ IATA="JED", Name="Jeddah",       City="Jeddah",      Country="Saudi Arabia",  TimeZone="Asia/Riyadh" },
                    new Airport{ IATA="RUH", Name="Riyadh",       City="Riyadh",      Country="Saudi Arabia",  TimeZone="Asia/Riyadh" },
                    new Airport{ IATA="IST", Name="Istanbul",     City="Istanbul",    Country="Türkiye",       TimeZone="Europe/Istanbul" },
                    new Airport{ IATA="LHR", Name="Heathrow",     City="London",      Country="UK",            TimeZone="Europe/London" },
                    new Airport{ IATA="KWI", Name="Kuwait",       City="Kuwait City", Country="Kuwait",        TimeZone="Asia/Kuwait" },
                    new Airport{ IATA="AMM", Name="Queen Alia",   City="Amman",       Country="Jordan",        TimeZone="Asia/Amman" },
                };

                var add = baseAirports
                    .Where(a => !db.Airports.Any(x => x.IATA == a.IATA))
                    .ToList();

                if (add.Count > 0)
                {
                    db.Airports.AddRange(add);
                    db.SaveChanges();
                }
            }

            // ---------------- Aircraft (>=10) ----------------
            if (db.Aircraft.Count() < 10)
            {
                var toAdd = new List<Aircraft>();
                int start = db.Aircraft.Count() + 1;
                while (start <= 10)
                {
                    toAdd.Add(new Aircraft
                    {
                        TailNumber = $"A{start:000}",
                        Model = (start % 2 == 0) ? "A320" : "B737-800",
                        Capacity = rnd.Next(150, 201)
                    });
                    start++;
                }
                if (toAdd.Count > 0)
                {
                    db.Aircraft.AddRange(toAdd);
                    db.SaveChanges();
                }
            }

            // ---------------- Crew (>=20) ----------------
            if (db.CrewMembers.Count() < 20)
            {
                var roles = new[] { "Captain", "FO", "Purser", "FA" };
                int start = db.CrewMembers.Count() + 1;
                var add = new List<CrewMember>();
                while (start <= 20)
                {
                    var role = roles[start % roles.Length];
                    add.Add(new CrewMember
                    {
                        FullName = $"Crew {start}",
                        Role = role,
                        LicenseNo = (role == "Captain" || role == "FO") ? $"LIC{start:0000}" : null
                    });
                    start++;
                }
                if (add.Count > 0)
                {
                    db.CrewMembers.AddRange(add);
                    db.SaveChanges();
                }
            }

            // ---------------- Routes (>=20) ----------------
            if (db.Routes.Count() < 20)
            {
                var apIds = db.Airports.Select(a => a.AirportId).ToList();
                var seen = new HashSet<(int o, int d)>();
                var add = new List<Route>();

                while (db.Routes.Count() + add.Count < 20)
                {
                    var o = apIds[rnd.Next(apIds.Count)];
                    var d = apIds[rnd.Next(apIds.Count)];
                    if (o == d) continue;
                    var key = (o, d);
                    if (!seen.Add(key)) continue;

                    add.Add(new Route
                    {
                        OriginAirportId = o,
                        DestinationAirportId = d,
                        DistanceKm = rnd.Next(300, 3200)
                    });
                }
                if (add.Count > 0)
                {
                    db.Routes.AddRange(add);
                    db.SaveChanges();
                }
            }

            // ---------------- Flights (>=30) ----------------
            if (db.Flights.Count() < 30)
            {
                var routes = db.Routes.AsNoTracking().ToList();
                var aircraft = db.Aircraft.AsNoTracking().ToList();

                var startDate = DateTime.UtcNow.Date.AddDays(-15);
                var endDate = DateTime.UtcNow.Date.AddDays(30);

                var add = new List<Flight>();
                var usedFn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                while (db.Flights.Count() + add.Count < 30)
                {
                    var r = routes[rnd.Next(routes.Count)];
                    var ac = aircraft[rnd.Next(aircraft.Count)];

                    var dep = startDate.AddDays(rnd.Next((endDate - startDate).Days))
                                       .AddHours(rnd.Next(6, 22))
                                       .AddMinutes(rnd.Next(0, 60));
                    var arr = dep.AddMinutes(rnd.Next(90, 420));

                    var fn = $"FM{rnd.Next(100, 999)}";
                    // keep flight number unique-ish in this seed batch
                    if (!usedFn.Add(fn)) continue;

                    add.Add(new Flight
                    {
                        FlightNumber = fn,
                        DepartureTime = dep,
                        ArrivalTime = arr,
                        AircraftId = ac.AircraftId,
                        RouteId = r.RouteId,
                        Status = "Scheduled"
                    });
                }

                if (add.Count > 0)
                {
                    db.Flights.AddRange(add);
                    db.SaveChanges();
                }
            }

            // ---------------- Passengers (>=50) ----------------
            if (db.Passengers.Count() < 50)
            {
                var nats = new[] { "OM", "AE", "SA", "QA", "BH", "KW", "IN", "PK" };
                int start = db.Passengers.Count() + 1;
                var add = new List<Passenger>();
                while (start <= 50)
                {
                    add.Add(new Passenger
                    {
                        FullName = $"Passenger {start}",
                        PassportNo = $"P{start:000000}",
                        Nationality = nats[rnd.Next(nats.Length)],
                        DOB = new DateTime(rnd.Next(1975, 2005), rnd.Next(1, 12), rnd.Next(1, 28))
                    });
                    start++;
                }
                if (add.Count > 0)
                {
                    db.Passengers.AddRange(add);
                    db.SaveChanges();
                }
            }

            // ---------------- Bookings (>=100) & Tickets (>=200) ----------------
            var seatLetters = new[] { "A", "B", "C", "D", "E", "F" };
            string NextSeat(HashSet<string> used, int capacity)
            {
                int rows = Math.Max(1, capacity / 6 + (capacity % 6 == 0 ? 0 : 1));
                for (int r = 1; r <= rows; r++)
                {
                    for (int c = 0; c < 6; c++)
                    {
                        var s = $"{r}{seatLetters[c]}";
                        if (used.Add(s)) return s;
                    }
                }
                return "NA";
            }

            if (db.Bookings.Count() < 100 || db.Tickets.Count() < 200)
            {
                var flights = db.Flights.Include(f => f.Aircraft).AsNoTracking().ToList();
                var pax = db.Passengers.AsNoTracking().ToList();

                var bookingsToAdd = new List<Booking>();
                var ticketsToAdd = new List<Ticket>();

                while (db.Bookings.Count() + bookingsToAdd.Count < 100
                    || db.Tickets.Count() + ticketsToAdd.Count < 200)
                {
                    var p = pax[rnd.Next(pax.Count)];

                    string MakeRef()
                        => $"BK{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

                    var b = new Booking
                    {
                        PassengerId = p.PassengerId,
                        BookingRef = MakeRef(),
                        BookingDate = DateTime.UtcNow.AddDays(-rnd.Next(1, 40)),
                        Status = "Confirmed"
                    };
                    bookingsToAdd.Add(b);

                    int legs = rnd.Next(1, 4);
                    for (int l = 0; l < legs && (db.Tickets.Count() + ticketsToAdd.Count) < 200; l++)
                    {
                        var fl = flights[rnd.Next(flights.Count)];
                        int cap = fl.Aircraft!.Capacity;

                        // seats used for this flight (consider already-added tickets in this batch too)
                        var used = new HashSet<string>(
                            db.Tickets.Where(t => t.FlightId == fl.FlightId).Select(t => t.SeatNumber),
                            StringComparer.OrdinalIgnoreCase);

                        foreach (var t in ticketsToAdd.Where(t => t.FlightId == fl.FlightId))
                            used.Add(t.SeatNumber);

                        var seat = NextSeat(used, cap);

                        ticketsToAdd.Add(new Ticket
                        {
                            Booking = b,
                            FlightId = fl.FlightId,
                            SeatNumber = seat,
                            Fare = Math.Round((decimal)rnd.Next(60, 500) + (decimal)rnd.NextDouble(), 2),
                            CheckedIn = rnd.Next(2) == 0
                        });
                    }
                }

                if (bookingsToAdd.Count > 0) db.Bookings.AddRange(bookingsToAdd);
                if (ticketsToAdd.Count > 0) db.Tickets.AddRange(ticketsToAdd);
                db.SaveChanges();
            }

            // ---------------- Baggage (>=150) ----------------
            if (db.Baggage.Count() < 150)
            {
                var ticketIds = db.Tickets.Select(t => t.TicketId).ToList();
                var bags = new List<Baggage>();
                while (db.Baggage.Count() + bags.Count < 150)
                {
                    var tid = ticketIds[rnd.Next(ticketIds.Count)];
                    bool overweight = rnd.Next(10) == 0; // ~10%
                    var w = overweight ? rnd.Next(31, 36) : rnd.Next(7, 28);
                    bags.Add(new Baggage
                    {
                        TicketId = tid,
                        WeightKg = Math.Round((decimal)(w + rnd.NextDouble()), 2),
                        TagNumber = $"T{tid:000000}{rnd.Next(10, 99)}"
                    });
                }
                if (bags.Count > 0)
                {
                    db.Baggage.AddRange(bags);
                    db.SaveChanges();
                }
            }

            // ---------------- Maintenance (>=15) ----------------
            if (db.Maintenance.Count() < 15)
            {
                var ac = db.Aircraft.AsNoTracking().ToList();
                var m = new List<AircraftMaintenance>();
                while (db.Maintenance.Count() + m.Count < 15)
                {
                    m.Add(new AircraftMaintenance
                    {
                        AircraftId = ac[rnd.Next(ac.Count)].AircraftId,
                        MaintenanceDate = DateTime.UtcNow.AddDays(-rnd.Next(10, 200)),
                        Type = (rnd.Next(2) == 0) ? "A-Check" : "B-Check",
                        Notes = "Seeded"
                    });
                }
                if (m.Count > 0)
                {
                    db.Maintenance.AddRange(m);
                    db.SaveChanges();
                }
            }

            // ---------------- Crew assignments (3–6 per flight) ----------------
            // Materialize data to avoid open reader issues
            var flightsAll = db.Flights.Include(f => f.FlightCrews).ToList();
            var crewAll = db.CrewMembers.AsNoTracking().ToList();

            var toAssign = new List<FlightCrew>();
            foreach (var fl in flightsAll)
            {
                if (fl.FlightCrews.Count >= 3) continue; // already has enough

                int n = rnd.Next(3, 7);
                var pick = crewAll.OrderBy(_ => Guid.NewGuid()).Take(n).ToList();
                var existing = fl.FlightCrews.Select(fc => fc.CrewId).ToHashSet();

                foreach (var cm in pick)
                {
                    if (!existing.Add(cm.CrewId)) continue;
                    toAssign.Add(new FlightCrew
                    {
                        FlightId = fl.FlightId,
                        CrewId = cm.CrewId,
                        RoleOnFlight = cm.Role
                    });
                }
            }
            if (toAssign.Count > 0)
            {
                db.FlightCrews.AddRange(toAssign);
                db.SaveChanges();
            }
        }
    }
}
