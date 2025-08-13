using System.Globalization;

namespace Flightmanagement.Crud;

internal static class Prompt
{
    public static string Ask(string label, string? def = null, bool required = true)
    {
        while (true)
        {
            Console.Write(def is null ? $"{label}: " : $"{label} [{def}]: ");
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s))
            {
                if (!required) return def ?? "";
                if (def is not null) return def;
                continue;
            }
            return s.Trim();
        }
    }

    public static DateTime AskDate(string label, DateTime? def = null)
    {
        while (true)
        {
            Console.Write(def is null ? $"{label} (yyyy-MM-dd): " : $"{label} (yyyy-MM-dd) [{def:yyyy-MM-dd}]: ");
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s) && def is not null) return def.Value;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            Console.WriteLine("Invalid date.");
        }
    }
}
