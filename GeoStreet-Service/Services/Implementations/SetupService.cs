using GeoStreet_Service.Repository;
using GeoStreet_Service.Services.Interfaces;

namespace GeoStreet_Service.Services.Implementations
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
