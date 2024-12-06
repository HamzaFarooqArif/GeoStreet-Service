using GeoStreet.API.Data;
using GeoStreet.API.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;

namespace GeoStreet.API.Respository
{
    public class StreetRepository : IStreetRepository
    {
        private readonly StreetDbContext _context;
        private readonly IConfiguration _configuration;

        public StreetRepository(StreetDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
            bool UseDatabaseLevelOperation = _configuration.GetValue<bool>("SpatialSettings:UsePostGIS"); ;
            try
            {
                if (UseDatabaseLevelOperation)
                {
                    // Perform database-level operation
                    return await AddPointDatabaseLevelAsync(streetId, newCoordinate, addToEnd);
                }
                else
                {
                    // Perform code-level operation
                    return await AddPointCodeLevelAsync(streetId, newCoordinate, addToEnd);
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new InvalidOperationException($"Operation failed: Street with ID {streetId} was not found.", ex);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException($"Concurrency conflict: The street with ID {streetId} was updated by another transaction.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error: Unable to update the geometry for Street ID {streetId}.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error: {ex.Message}", ex);
            }
        }

        private async Task<bool> AddPointDatabaseLevelAsync(int streetId, Coordinate newCoordinate, bool addToEnd)
        {
            // Define a separate transaction scope
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch and lock the row with FOR UPDATE
                var geometry = await _context.Streets
                    .FromSqlInterpolated($"SELECT * FROM \"Streets\" WHERE \"Id\" = {streetId} FOR UPDATE")
                    .Select(s => s.Geometry)
                    .FirstOrDefaultAsync();

                if (geometry == null)
                {
                    throw new KeyNotFoundException($"Street with ID {streetId} does not exist.");
                }

                var srid = _configuration.GetValue<int>("SpatialSettings:DefaultSRID");

                // Perform the geometry update
                var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE ""Streets""
                       SET ""Geometry"" = ST_AddPoint(
                           ""Geometry"",
                           ST_SetSRID(ST_MakePoint({newCoordinate.X}, {newCoordinate.Y}), {srid}),
                           {(addToEnd ? -1 : 0)}
                       )
                       WHERE ""Id"" = {streetId}");

                // Commit the transaction
                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Generic exception for unexpected issues
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<bool> AddPointCodeLevelAsync(int streetId, Coordinate newCoordinate, bool addToEnd)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var srid = _configuration.GetValue<int>("SpatialSettings:DefaultSRID");

                // Fetch the street and its geometry
                var street = await _context.Streets.FindAsync(streetId);
                if (street == null)
                {
                    throw new KeyNotFoundException($"Street with ID {streetId} does not exist.");
                }

                if (street.Geometry == null)
                {
                    // Initialize geometry if null
                    street.Geometry = new LineString(new[] { newCoordinate }) { SRID = srid };
                }
                else
                {
                    // Append or prepend the new coordinate
                    if (addToEnd)
                    {
                        street.Geometry = new LineString(street.Geometry.Coordinates.Append(newCoordinate).ToArray()) { SRID = srid };
                    }
                    else
                    {
                        street.Geometry = new LineString(new[] { newCoordinate }.Concat(street.Geometry.Coordinates).ToArray()) { SRID = srid };
                    }
                }

                // Save changes to the database
                _context.Streets.Update(street);
                var result = await _context.SaveChangesAsync() > 0;

                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
