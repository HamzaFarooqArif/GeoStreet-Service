using GeoStreet.API.Models.DomainModels;
using GeoStreet.API.Respository;
using GeoStreet.API.Services.Interfaces;

namespace GeoStreet.API.Services.Implementations
{
    public class StreetService : IStreetService
    {
        private readonly IStreetRepository _repository;

        public StreetService(IStreetRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Street>> GetAllStreetsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Street> GetStreetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task CreateStreetAsync(Street street)
        {
            await _repository.AddAsync(street);
        }

        public async Task UpdateStreetAsync(Street street)
        {
            await _repository.UpdateAsync(street);
        }

        public async Task DeleteStreetAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
