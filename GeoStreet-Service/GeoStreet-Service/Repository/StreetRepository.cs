using GeoStreet_Service.Data;
using GeoStreet_Service.Models.DomainModels;
using Microsoft.EntityFrameworkCore;

namespace GeoStreet_Service.Repository
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
    }
}
