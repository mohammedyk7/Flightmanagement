using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flightmanagement.Interfaces
{
    // Lightweight DTOs that the console prints use
    public record DailyManifestDto(
        string FlightNumber,
        DateTime DepartureUtc,
        DateTime ArrivalUtc,
        string OriginIATA,
        string DestIATA,
        string AircraftTail,
        int PassengerCount,
        List<string> CrewList,
        decimal TotalBaggageWeight
    );

    public record RouteRevenueDto(
        string RouteCode,
        int FlightCount,
        int SeatsSold,
        decimal AvgFare,
        decimal TotalRevenue
    );

    public record OnTimeDto(
        string Key,
        int OnTimeCount,
        int TotalCount,
        decimal OnTimePct
    );

    public record SeatOccDto(
        string FlightNumber,
        DateTime DepartureDate,
        decimal OccupancyPct,
        string Origin,
        string Destination
    );

    public record CrewConflictDto(
        int CrewId,
        string CrewName,
        string FlightA,
        string FlightB,
        string ConflictDetail
    );

    public record ConnectionDto(string PassengerName, string Itinerary);
    public record FrequentFlierDto(string PassengerName, int FlightCount, double TotalDistance);
    public record MaintenanceAlertDto(string TailNumber, DateTime DueDate, double CumulativeHours, string Note);
    public record OverweightDto(string BookingRef, int TicketId, decimal TotalWeightKg);

    public record SetOpsDto(
        IEnumerable<string> UnionSample,
        IEnumerable<string> IntersectSample,
        IEnumerable<string> ExceptSample
    );

    public record FlightPageDto(
        string FlightNumber,
        DateTime DepartureDate,
        string Origin,
        string Destination
    );

    public record RunningRevenueDto(DateTime Date, decimal Revenue, decimal RunningTotal);
    public record ForecastDto(DateTime Date, decimal Revenue);

    public interface IFlightQueryService
    {
        Task<IReadOnlyList<DailyManifestDto>> GetDailyManifestAsync(DateTime date);
        Task<IReadOnlyList<RouteRevenueDto>> GetTopRoutesByRevenueAsync(DateTime from, DateTime to, int topN);
        Task<IReadOnlyList<OnTimeDto>> GetOnTimePerformanceAsync(DateTime from, DateTime to, int thresholdMinutes);
        Task<IReadOnlyList<SeatOccDto>> GetSeatOccupancyAsync();
        Task<IReadOnlyList<string>> GetAvailableSeatNumbersAsync(int flightId);
        Task<IReadOnlyList<CrewConflictDto>> GetCrewSchedulingConflictsAsync();
        Task<IReadOnlyList<ConnectionDto>> GetPassengersWithConnectionsAsync(int maxLayoverHours);
        Task<IReadOnlyList<FrequentFlierDto>> GetFrequentFliersAsync(int minFlights);
        Task<IReadOnlyList<MaintenanceAlertDto>> GetMaintenanceAlertsAsync(DateTime until);
        Task<IReadOnlyList<OverweightDto>> GetBaggageOverweightAlertsAsync(decimal perTicketLimitKg);
        Task<SetOpsDto> GetSetOperationSamplesAsync();
        Task<IReadOnlyList<FlightPageDto>> PageFlightsAsync(int page, int pageSize);
        Task<Dictionary<string, int>> FlightsByNumberMapAsync();
        Task<RouteRevenueDto[]> TopRoutesToArrayAsync(int topN);
        Task<int> AsEnumerableSampleCountAsync();
        Task<int> OfTypeSampleAsync();
        Task<IReadOnlyList<RunningRevenueDto>> GetRunningDailyRevenueAsync(int days);
        public Task<IReadOnlyList<ForecastDto>> ForecastNextNDaysAsync(int days)
        {
            var start = DateTime.UtcNow.Date.AddDays(1);
            var baseRevenue = 12000m;      // tweak scale if you like
            var growthPerDay = 0.01m;      // 1% daily growth
            var results = new List<ForecastDto>(Math.Max(1, days));

            for (int i = 0; i < Math.Max(1, days); i++)
            {
                var date = start.AddDays(i);

                // weekly seasonality in [-10%, +10%]
                var weeklyAngle = (double)(i % 7) / 7.0 * 2.0 * Math.PI;
                var seasonality = (decimal)(Math.Sin(weeklyAngle) * 0.10);

                // deterministic “noise” ~ [-2%, +2%]
                var rnd = new Random(HashCode.Combine(date.Year, date.Month, date.Day));
                var noise = (decimal)((rnd.NextDouble() - 0.5) * 0.04);

                var multiplier = 1m + (growthPerDay * i) + seasonality + noise;
                var revenue = decimal.Round(baseRevenue * multiplier, 2, MidpointRounding.AwayFromZero);
                if (revenue < 0m) revenue = baseRevenue * 0.5m;

                results.Add(new ForecastDto(date, revenue));
            }

            return Task.FromResult<IReadOnlyList<ForecastDto>>(results);
        }


    }
}
