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
                optionsBuilder.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            modelBuilder.Entity<Street>()
                .Property(s => s.Geometry)
                .HasConversion(
                    // Convert LineString to WKT for storage
                    v => v == null ? null : v.AsText(),
                    // Convert WKT back to LineString for retrieval
                    v => v == null ? null : (LineString)new WKTReader(geometryFactory).Read(v)
                )
                .HasColumnType("TEXT"); // Store as TEXT for SQLite compatibility
        }

        public DbSet<Street> Streets { get; set; }
    }
}
