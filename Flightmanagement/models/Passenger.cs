using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Flightmanagement.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        [Required] public string FullName { get; set; } = "";
        [Required] public string PassportNo { get; set; } = "";
        public string Nationality { get; set; } = "";
        public DateTime DOB { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
