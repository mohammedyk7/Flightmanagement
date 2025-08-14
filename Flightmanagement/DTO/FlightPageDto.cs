using System;

namespace Flightmanagement.DTOs
{
    public class FlightPageDto
    {
        public string FlightNumber { get; set; } = "";
        public DateTime DepartureDate { get; set; }
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
    }
}
