using Microsoft.EntityFrameworkCore;
using Flightmanagement.Models;        // <— the namespace where Passenger, Booking, etc. are declared

using Flightmanagement.Data;   // <-- your namespace where FlightContext & entities live

namespace Flightmanagement.Crud;

internal static class PassengerCrud
{
    public static async Task RunAsync(FlightContext db)
    {
        while (true)
        {
            Console.WriteLine("\n=== Passenger CRUD ===");
            Console.WriteLine("1) List (top 20)");
            Console.WriteLine("2) Details");
            Console.WriteLine("3) Create");
            Console.WriteLine("4) Update");
            Console.WriteLine("5) Delete");
            Console.WriteLine("0) Back");
            Console.Write("> ");
            var k = Console.ReadLine();

            switch (k)
            {
                case "1": await ListAsync(db); break;
                case "2": await DetailsAsync(db); break;
                case "3": await CreateAsync(db); break;
                case "4": await UpdateAsync(db); break;
                case "5": await DeleteAsync(db); break;
                case "0": return;
            }
        }
    }

    private static async Task ListAsync(FlightContext db)
    {
        var items = await db.Passengers
            .AsNoTracking()
            .OrderByDescending(p => p.PassengerId)
            .Take(20)
            .ToListAsync();

        Console.WriteLine("\nId  Name                              Passport       Nationality   DOB");
        Console.WriteLine(new string('-', 78));
        foreach (var p in items)
            Console.WriteLine($"{p.PassengerId,2}  {p.FullName,-32}  {p.PassportNo,-12}  {p.Nationality,-12}  {p.DOB:yyyy-MM-dd}");
    }

    private static async Task DetailsAsync(FlightContext db)
    {
        Console.Write("Passenger id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) return;

        var p = await db.Passengers
            .Include(x => x.Bookings)
            .ThenInclude(b => b.Tickets)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.PassengerId == id);

        if (p is null) { Console.WriteLine("Not found."); return; }

        Console.WriteLine($"\n#{p.PassengerId}  {p.FullName}");
        Console.WriteLine($"Passport: {p.PassportNo}   Nationality: {p.Nationality}   DOB: {p.DOB:yyyy-MM-dd}");
        Console.WriteLine($"Bookings: {p.Bookings?.Count ?? 0}");
    }

    private static async Task CreateAsync(FlightContext db)
    {
        var fullName = Prompt.Ask("Full name");
        var passportNo = Prompt.Ask("Passport no");
        var nationality = Prompt.Ask("Nationality");
        var dob = Prompt.AskDate("DOB");

        // unique passport check
        var exists = await db.Passengers.AnyAsync(x => x.PassportNo == passportNo);
        if (exists) { Console.WriteLine("❌ A passenger with that passport already exists."); return; }

        var p = new Passenger
        {
            FullName = fullName,
            PassportNo = passportNo,
            Nationality = nationality,
            DOB = dob
        };

        await db.Passengers.AddAsync(p);
        await SaveAsync(db, "✅ Created passenger", $"#{p.PassengerId}");
    }

    private static async Task UpdateAsync(FlightContext db)
    {
        Console.Write("Passenger id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) return;

        var p = await db.Passengers.FirstOrDefaultAsync(x => x.PassengerId == id);
        if (p is null) { Console.WriteLine("Not found."); return; }

        p.FullName = Prompt.Ask("Full name", p.FullName);
        var newPass = Prompt.Ask("Passport no", p.PassportNo);
        if (!string.Equals(newPass, p.PassportNo, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await db.Passengers.AnyAsync(x => x.PassportNo == newPass && x.PassengerId != id);
            if (exists) { Console.WriteLine("❌ Passport already used by another passenger."); return; }
            p.PassportNo = newPass;
        }
        p.Nationality = Prompt.Ask("Nationality", p.Nationality);
        p.DOB = Prompt.AskDate("DOB", p.DOB);

        db.Passengers.Update(p);
        await SaveAsync(db, "✅ Updated passenger", $"#{p.PassengerId}");
    }

    private static async Task DeleteAsync(FlightContext db)
    {
        Console.Write("Passenger id: ");
        if (!int.TryParse(Console.ReadLine(), out var id)) return;

        var p = await db.Passengers
            .Include(x => x.Bookings)
            .ThenInclude(b => b.Tickets)
            .ThenInclude(t => t.Baggage)
            .FirstOrDefaultAsync(x => x.PassengerId == id);

        if (p is null) { Console.WriteLine("Not found."); return; }

        // protect if they have bookings – ask before cascading
        if (p.Bookings?.Any() == true)
        {
            Console.WriteLine($"Passenger has {p.Bookings.Count} booking(s). Delete all related data? (y/N)");
            var yn = Console.ReadLine();
            if (!string.Equals(yn, "y", StringComparison.OrdinalIgnoreCase)) return;

            // remove children (in case cascade is disabled)
            var allBaggage = p.Bookings.SelectMany(b => b.Tickets).SelectMany(t => t.Baggage);
            db.Baggage.RemoveRange(allBaggage);
            db.Tickets.RemoveRange(p.Bookings.SelectMany(b => b.Tickets));
            db.Bookings.RemoveRange(p.Bookings);
        }

        db.Passengers.Remove(p);
        await SaveAsync(db, "✅ Deleted passenger", $"#{p.PassengerId}");
    }

    private static async Task SaveAsync(FlightContext db, string okMsg, string tail = "")
    {
        try
        {
            await db.SaveChangesAsync();
            Console.WriteLine($"{okMsg} {tail}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("❌ DB error: " + ex.GetBaseException().Message);
        }
    }
}
