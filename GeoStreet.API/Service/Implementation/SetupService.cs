using GeoStreet.API.Respository;
using GeoStreet.API.Services.Interfaces;

namespace GeoStreet.API.Services.Implementations
{
    public class SetupService : ISetupService
    {
        private readonly IStreetRepository _repository;
        public SetupService(IStreetRepository repository)
        {
            _repository = repository;
        }
        public async Task ApplyDatabaseMigrations()
        {
            await _repository.ApplyDatabaseMigrations();
        }
    }
}
