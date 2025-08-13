using Flightmanagement.Models;

namespace Flightmanagement.Interfaces;

public interface ICrewRepository : IRepository<CrewMember>
{
    Task<List<CrewMember>> GetByRoleAsync(string role);
}
