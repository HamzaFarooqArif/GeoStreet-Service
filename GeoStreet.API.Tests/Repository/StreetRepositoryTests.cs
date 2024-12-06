

using GeoStreet.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GeoStreet.API.Respository;
using GeoStreet.API.Services.Interfaces;
using GeoStreet.API.Models;
using GeoStreet.API.Services.Implementations;
using NetTopologySuite;
using GeoStreet.API.Models.DomainModels;
using NetTopologySuite.Geometries;
using Testcontainers.PostgreSql;

namespace GeoStreet.API.Tests.Repository
{
    [TestClass]
    public class StreetRepositoryTests
    {
        private IServiceProvider _serviceProvider;
        private PostgreSqlContainer _postgresContainer;

        [TestInitialize]
        public void Initialize()
        {
            _postgresContainer = new PostgreSqlBuilder()
              .WithImage("postgis/postgis:12-3.0")
              .Build();

            _postgresContainer.StartAsync().GetAwaiter().GetResult();

            var services = new ServiceCollection();
            services.AddDbContext<StreetDbContext>(options =>
                options.UseNpgsql(
                    _postgresContainer.GetConnectionString(),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsAssembly("GeoStreet.API") // Specify the assembly containing migrations
                        .UseNetTopologySuite()
                )
            );

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:WebApiDatabase", _postgresContainer.GetConnectionString() }
                })
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IStreetRepository, StreetRepository>();
            services.AddScoped<IStreetService, StreetService>();
            services.AddAutoMapper(typeof(MappingProfile));

            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<StreetDbContext>();

            if (!context.Database.CanConnect())
            {
                throw new InvalidOperationException("Unable to connect to the database.");
            }

            context.Database.EnsureCreated();
            SeedTestData().GetAwaiter().GetResult(); // Seed initial data for tests
        }

        private async Task SeedTestData()
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IStreetRepository>();

            // Create and seed the test street
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var street = new Street
            {
                Id = 1,
                Name = "Test Street",
                Geometry = factory.CreateLineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 1)
                })
            };

            await repository.AddAsync(street); // Save the street using the repository
        }

        [TestMethod]
        public async Task AddPointToStreetAsync_ShouldHandleConcurrency()
        {
            using var contextScope = _serviceProvider.CreateScope();
            var repository = contextScope.ServiceProvider.GetRequiredService<IStreetRepository>();

            using var scope1 = _serviceProvider.CreateScope();
            var repository1 = scope1.ServiceProvider.GetRequiredService<IStreetRepository>();

            using var scope2 = _serviceProvider.CreateScope();
            var repository2 = scope2.ServiceProvider.GetRequiredService<IStreetRepository>();

            var task1 = Task.Run(async () =>
            {
                await repository1.AddPointAsync(1, new Coordinate(2, 2), false);
            });

            var task2 = Task.Run(async () =>
            {
                await repository2.AddPointAsync(1, new Coordinate(3, 3), false);
            });

            await Task.WhenAll(task1, task2);

            // Assert: Verify database state
            var updatedStreet = await repository.GetByIdAsync(1);
            Assert.IsNotNull(updatedStreet);
            Assert.IsTrue(updatedStreet.Geometry.Coordinates.Length >= 3); // Ensure both points were added
        }
    }
}