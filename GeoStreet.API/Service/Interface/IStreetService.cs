using GeoStreet.API.Models.DomainModels;
using GeoStreet.API.Models.ViewModels;

namespace GeoStreet.API.Services.Interfaces
{
    public interface IStreetService
    {
        Task<IEnumerable<StreetViewModel>> GetAllStreetsAsync();
        Task<StreetViewModel> GetStreetByIdAsync(int id);
        Task CreateStreetAsync(StreetViewModel street);
        Task UpdateStreetAsync(StreetViewModel street);
        Task DeleteStreetAsync(int id);
        Task<bool> AddPointToStreetAsync(int id, AddPointRequest request);
    }
}
