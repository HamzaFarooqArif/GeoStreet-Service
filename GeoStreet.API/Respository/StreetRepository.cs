using GeoStreet.API.Data;
using GeoStreet.API.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace GeoStreet.API.Respository
{
    public class StreetRepository : IStreetRepository
    {
        private readonly StreetDbContext _context;

        public StreetRepository(StreetDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Street>> GetAllAsync()
        {
            return await _context.Streets.ToListAsync();
        }

        public async Task<Street> GetByIdAsync(int id)
        {
            return await _context.Streets.FindAsync(id);
        }
        public async Task AddAsync(Street street)
        {
            await _context.Streets.AddAsync(street);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Street street)
        {
            _context.Streets.Update(street);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var street = await _context.Streets.FindAsync(id);
            if (street != null)
            {
                _context.Streets.Remove(street);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ApplyDatabaseMigrations()
        {
            await _context.Database.MigrateAsync();
        }

        public async Task<Coordinate[]> GetStartAndEndCoordinatesAsync(int id)
        {
            var result = await _context.Streets
                .Where(s => s.Id == id && s.Geometry != null)
                .Select(s => new
                {
                    Start = s.Geometry.Coordinates.FirstOrDefault(),
                    End = s.Geometry.Coordinates.LastOrDefault()
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return null;

            return new[] { result.Start, result.End };
        }

        public async Task<bool> AddPointAsync(int streetId, Coordinate newCoordinate, bool addToEnd)
        {
            // Perform the operation in a single query
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE ""Streets""
                   SET ""Geometry"" = 
                   CASE
                       WHEN ""Geometry"" IS NULL THEN 
                           ST_SetSRID(ST_MakePoint({newCoordinate.X}, {newCoordinate.Y})::geometry, 4326)
                       ELSE 
                           ST_AddPoint(
                               ""Geometry"",
                               ST_SetSRID(ST_MakePoint({newCoordinate.X}, {newCoordinate.Y}), 4326),
                               {(addToEnd ? -1 : 0)}
                           )
                   END
                   WHERE ""Id"" = {streetId}");

            // Check if the row was updated
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException($"Street with ID {streetId} does not exist.");
            }

            return rowsAffected > 0;
        }
    }
}
