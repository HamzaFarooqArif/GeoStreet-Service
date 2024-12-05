using GeoStreet.API.Models.DomainModels;
using NetTopologySuite.Geometries;

namespace GeoStreet.API.Respository
{
    public interface IStreetRepository
    {
        Task<IEnumerable<Street>> GetAllAsync();
        Task<Street> GetByIdAsync(int id);
        Task AddAsync(Street street);
        Task UpdateAsync(Street street);
        Task DeleteAsync(int id);
        Task<Coordinate[]> GetStartAndEndCoordinatesAsync(int id);
        Task<bool> AddPointAsync(int streetId, Coordinate newCoordinate, bool addToEnd);
        Task ApplyDatabaseMigrations();
    }
}
