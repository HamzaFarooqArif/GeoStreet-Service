using GeoStreet.API.Models.DomainModels;

namespace GeoStreet.API.Services.Interfaces
{
    public interface IStreetService
    {
        Task<IEnumerable<Street>> GetAllStreetsAsync();
        Task<Street> GetStreetByIdAsync(int id);
        Task CreateStreetAsync(Street street);
        Task UpdateStreetAsync(Street street);
        Task DeleteStreetAsync(int id);
    }
}
