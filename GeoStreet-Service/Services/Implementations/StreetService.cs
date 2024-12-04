using GeoStreet_Service.Models.DomainModels;
using GeoStreet_Service.Repository;
using GeoStreet_Service.Services.Interfaces;

namespace GeoStreet_Service.Services.Implementations
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
