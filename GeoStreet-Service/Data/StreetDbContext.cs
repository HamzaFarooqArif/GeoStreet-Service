﻿using GeoStreet_Service.Models.DomainModels;
using Microsoft.EntityFrameworkCore;
namespace GeoStreet_Service.Data
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