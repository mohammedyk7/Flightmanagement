using Microsoft.EntityFrameworkCore;
using Flightmanagement.Models;

namespace Flightmanagement.Data
{
    public class FlightContext : DbContext
    {
        public FlightContext() { }
        public FlightContext(DbContextOptions<FlightContext> options) : base(options) { }

        public DbSet<Airport> Airports => Set<Airport>();
        public DbSet<Aircraft> Aircraft => Set<Aircraft>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<Passenger> Passengers => Set<Passenger>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Baggage> Baggage => Set<Baggage>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<CrewMember> CrewMembers => Set<CrewMember>();
        public DbSet<FlightCrew> FlightCrews => Set<FlightCrew>();
        public DbSet<AircraftMaintenance> Maintenance => Set<AircraftMaintenance>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // NOTE: MARS enabled
                optionsBuilder.UseSqlServer(
                    @"Server=(localdb)\MSSQLLocalDB;Database=FlightManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Route configuration
            modelBuilder.Entity<Route>(entity =>
            {
                entity.HasKey(r => r.RouteId);

                entity.HasOne(r => r.OriginAirport)
                      .WithMany(a => a.OriginRoutes)
                      .HasForeignKey(r => r.OriginAirportId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.DestinationAirport)
                      .WithMany(a => a.DestinationRoutes)
                      .HasForeignKey(r => r.DestinationAirportId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Airport configuration
            modelBuilder.Entity<Airport>(entity =>
            {
                entity.HasKey(a => a.AirportId);
                entity.HasIndex(a => a.IATA).IsUnique();
            });

            // FlightCrew composite key
            modelBuilder.Entity<FlightCrew>(eb =>
            {
                eb.HasKey(fc => new { fc.FlightId, fc.CrewId });

                eb.HasOne(fc => fc.Flight)
                  .WithMany(f => f.FlightCrews)
                  .HasForeignKey(fc => fc.FlightId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(fc => fc.Crew)
                  .WithMany(c => c.FlightCrews)
                  .HasForeignKey(fc => fc.CrewId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // Fix decimal warnings
            modelBuilder.Entity<Ticket>()
                .Property(t => t.Fare)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Baggage>()
                .Property(b => b.WeightKg)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.IATA)
                .IsUnique();



            modelBuilder.Entity<Flight>()
                  .HasIndex(f => new { f.FlightNumber, f.DepartureTime })
                   .IsUnique();

            modelBuilder.Entity<Passenger>()
                .HasIndex(p => p.PassportNo)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingRef)
                .IsUnique();

            modelBuilder.Entity<Aircraft>()
                .HasIndex(a => a.TailNumber)
                .IsUnique();


        }
    }
}
