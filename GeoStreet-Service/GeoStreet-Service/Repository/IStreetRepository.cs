using GeoStreet_Service.Models.DomainModels;

namespace GeoStreet_Service.Repository
{
    public interface IStreetRepository
    {
        Task<IEnumerable<Street>> GetAllAsync();
        Task<Street> GetByIdAsync(int id);
        Task AddAsync(Street street);
        Task UpdateAsync(Street street);
        Task DeleteAsync(int id);
    }
}
