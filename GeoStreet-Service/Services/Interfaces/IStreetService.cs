using GeoStreet_Service.Models.DomainModels;

namespace GeoStreet_Service.Services.Interfaces
{
    public interface IStreetService
    {
        Task<IEnumerable<Street>> GetAllStreetsAsync();
        Task<Street> GetStreetByIdAsync(int id);
        Task CreateStreetAsync(Street street);
        Task UpdateStreetAsync(Street street);
        Task DeleteStreetAsync(int id);
        Task ApplyDatabaseMigrations();
    }
}
