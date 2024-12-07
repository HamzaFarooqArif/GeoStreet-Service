using GeoStreet.API.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;

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
            // Read the flag from configuration
            bool usePostGIS = _configuration.GetValue<bool>("FeatureFlags:UsePostGIS");
            if (usePostGIS)
            {
                optionsBuilder.UseNpgsql(o => o.UseNetTopologySuite());
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Read the flag from configuration
            bool usePostGIS = _configuration.GetValue<bool>("FeatureFlags:UsePostGIS");

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(
                srid: _configuration.GetValue<int>("SpatialSettings:DefaultSRID")
            );

            modelBuilder.Entity<Street>(entity =>
            {
                if (usePostGIS)
                {
                    // Configure geometry as a PostGIS column
                    entity.Property(e => e.Geometry)
                        .HasColumnType("geometry") // Use PostGIS geometry type
                        .HasConversion(
                            v => v, // No conversion needed
                            v => v
                        )
                        .IsRequired(false);
                }
                else
                {
                    // Configure geometry as a simple text column
                    entity.Property(e => e.Geometry)
                        .HasColumnType("TEXT") // Use simple text column
                        .HasConversion(
                            v => v == null ? null : v.AsText(), // Convert LineString to WKT for storage
                            v => v == null ? null : (LineString)new WKTReader(geometryFactory).Read(v) // Convert WKT to LineString
                        )
                        .IsRequired(false);
                }
            });
        }

        public DbSet<Street> Streets { get; set; }
    }
}
