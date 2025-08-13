namespace Flightmanagement.Repositories;

using Flightmanagement.Data;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;
using Microsoft.EntityFrameworkCore;

public class CrewRepository : Repository<CrewMember>, ICrewRepository
{
    public CrewRepository(FlightContext db) : base(db) { }

    public Task<List<CrewMember>> GetByRoleAsync(string role) =>
        _set.AsNoTracking()
            .Where(c => c.Role == role)
            .ToListAsync();
}
