using GeoStreet.API.Models.DomainModels;

namespace GeoStreet.API.Respository
{
    public interface IStreetRepository
    {
        Task<IEnumerable<Street>> GetAllAsync();
        Task<Street> GetByIdAsync(int id);
        Task AddAsync(Street street);
        Task UpdateAsync(Street street);
        Task DeleteAsync(int id);
        Task ApplyDatabaseMigrations();
    }
}
