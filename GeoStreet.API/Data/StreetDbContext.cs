using GeoStreet.API.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;

namespace GeoStreet.API.Data
{
    public class StreetDbContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public StreetDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(_configuration.GetConnectionString("WebApiDatabase"));
        }
        public DbSet<Street> Streets { get; set; }
    }
}
