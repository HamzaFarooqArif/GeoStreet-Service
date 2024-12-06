using GeoStreet.API.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite;
using System.Collections.Generic;
using System.IO;

namespace GeoStreet.API.Data
{
    public class StreetDbContext : DbContext
    {
        protected readonly IConfiguration _configuration;
        public StreetDbContext(DbContextOptions<StreetDbContext> options, IConfiguration configuration): base(options)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use PostgreSQL for production by default
                var connectionString = _configuration.GetConnectionString("WebApiDatabase");
            }
            optionsBuilder.UseNpgsql(o => o.UseNetTopologySuite());
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Street> Streets { get; set; }
    }
}
