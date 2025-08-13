using System.Collections.Generic;
using System.Threading.Tasks;
using Flightmanagement.Interfaces;
using Flightmanagement.Models;

namespace Flightmanagement.Services
{
    public class AirportService : IAirportService
    {
        private readonly IAirportRepository _repo;

        public AirportService(IAirportRepository repo) => _repo = repo;

        // Matches interface: 5 string parameters (names here are illustrative).
        // We safely ignore fields your Airport model doesn't have.
        public async Task<Airport> CreateAirportAsync(string iata, string name, string icao, string city, string country)
        {
            var a = new Airport
            {
                IATA = iata,
                Name = name
                // If your model also has ICAO/City/Country, set them here:
                // ICAO = icao,
                // City = city,
                // Country = country
            };

            await _repo.AddAsync(a);
            await _repo.SaveAsync();
            return a;
        }

        public Task<List<Airport>> ListAsync() => _repo.ListAsync();

        public Task<Airport?> GetAsync(int id) => _repo.GetByIdAsync(id);

        public Task<Airport?> FindByIataAsync(string iata) => _repo.FindByIataAsync(iata);

        public async Task DeleteAirportAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return;

            _repo.Delete(entity);    // repo.Delete expects Airport, not int
            await _repo.SaveAsync();
        }

        // Optional: simple rename helper
        public async Task<Airport?> UpdateAirportNameAsync(int id, string newName)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Name = newName;
            _repo.Update(entity);
            await _repo.SaveAsync();
            return entity;
        }
    }
}
