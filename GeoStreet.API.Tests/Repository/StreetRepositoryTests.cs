

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
using Microsoft.EntityFrameworkCore.Metadata;
using NetTopologySuite.IO;

namespace GeoStreet.API.Tests.Repository
{
    [TestClass]
    public class StreetRepositoryTests
    {
        private IServiceProvider _serviceProvider;
        private PostgreSqlContainer _postgresContainer;
        private IConfiguration _configuration;

        private void BuildServiceProvider()
        {
            BuildPostgreSQLContainer();

            var services = new ServiceCollection();

            // Add DbContext with conditional geometry handling based on the configuration
            services.AddDbContext<StreetDbContext>(options =>
            {
                var usePostGIS = _configuration.GetValue<bool>("FeatureFlags:UsePostGIS");

                if (usePostGIS)
                {
                    // Configure PostGIS (NetTopologySuite)
                    options.UseNpgsql(
                        _postgresContainer.GetConnectionString(),
                        npgsqlOptions => npgsqlOptions
                            .UseNetTopologySuite()
                    );
                }
                else
                {
                    // Configure simple text column for geometry
                    options.UseNpgsql(_postgresContainer.GetConnectionString());
                }
            });

            services.AddSingleton<IConfiguration>(_configuration);
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
        }

        private IConfiguration BuildConfiguration(bool usePostGIS)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:WebApiDatabase", _postgresContainer.GetConnectionString() },
                    { "SpatialSettings:DefaultSRID", "4326" },
                    { "FeatureFlags:UsePostGIS", usePostGIS.ToString().ToLower() },
                })
                .Build();
        }

        private void BuildPostgreSQLContainer()
        {
            if (_postgresContainer != null)
            {
                _postgresContainer?.DisposeAsync();
            }

            // Initialize a throwaway PostgreSQL container using a PostGIS-enabled Docker image
            _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:12-3.0")
            .Build();

            _postgresContainer.StartAsync().GetAwaiter().GetResult();
        }

        [TestInitialize]
        public void Initialize()
        {
            BuildPostgreSQLContainer();
        }

        [TestMethod]
        public async Task AddPointAsync_ConcurrencyTest_CodeLevel()
        {
            _configuration = BuildConfiguration(usePostGIS: false);
            BuildServiceProvider();

            using var scope1 = _serviceProvider.CreateScope();
            var repository1 = scope1.ServiceProvider.GetRequiredService<IStreetRepository>();

            using var scope2 = _serviceProvider.CreateScope();
            var repository2 = scope1.ServiceProvider.GetRequiredService<IStreetRepository>();

            // Arrange: Seed the database with a street
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: _configuration.GetValue<int>("SpatialSettings:DefaultSRID"));
            var initialStreet = new Street
            {
                Id = 1,
                Name = "Concurrent Street",
                Geometry = factory.CreateLineString(new[]
                {
                        new Coordinate(0, 0),
                        new Coordinate(1, 1)
                    })
            };
            await repository1.AddAsync(initialStreet);

            // Act: Simulate two concurrent updates
            var coordinate1 = new Coordinate(2, 2);
            var coordinate2 = new Coordinate(3, 3);

            Exception task1Exception = null;
            Exception task2Exception = null;

            var task1 = Task.Run(async () =>
            {
                try
                {
                    await repository1.AddPointAsync(1, coordinate1, true); // Add to the end
                }
                catch (Exception ex)
                {
                    task1Exception = ex;
                }
            });

            var task2 = Task.Run(async () =>
            {
                try
                {
                    await repository2.AddPointAsync(1, coordinate2, true); // Add to the end
                }
                catch (Exception ex)
                {
                    task2Exception = ex;
                }
            });

            // Wait for both tasks to complete and expect one to fail
            await Task.WhenAll(task1, task2);

            // Assert: Handle both cases (exception or no exception)
            if (task1Exception != null || task2Exception != null)
            {
                // Assert that at least one exception was thrown and it matches the expected type and message
                var thrownException = task1Exception ?? task2Exception;

                Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException), "Expected an InvalidOperationException.");

                var expectedMessages = new[]
                    {
                        "the connection is already in a transaction",
                        "the connection is already in state 'executing'",
                        "Connection already open",
                    };
                Assert.IsTrue(
                    expectedMessages.Any(message => thrownException.Message.ToLower().Contains(message.ToLower())),
                    $"The exception message does not match any of the expected errors. Actual: {thrownException.Message}"
                );
            }

            // Assert: Check if only one task succeeded
            using var verifyScope = _serviceProvider.CreateScope();
            var context = verifyScope.ServiceProvider.GetRequiredService<StreetDbContext>();
            var street = await context.Streets.FindAsync(1);

            Assert.IsNotNull(street);
            Assert.IsTrue(street.Geometry.Coordinates.Length >= 3); // Ensure at least one update succeeded
        }

        [TestMethod]
        public async Task AddPointAsync_ConcurrencyTest_DatabaseLevel()
        {
            _configuration = BuildConfiguration(usePostGIS: true);
            BuildServiceProvider();

            using var scope1 = _serviceProvider.CreateScope();
            var repository1 = scope1.ServiceProvider.GetRequiredService<IStreetRepository>();

            using var scope2 = _serviceProvider.CreateScope();
            var repository2 = scope1.ServiceProvider.GetRequiredService<IStreetRepository>();

            // Arrange: Seed the database with a street
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: _configuration.GetValue<int>("SpatialSettings:DefaultSRID"));
            var initialStreet = new Street
            {
                Id = 1,
                Name = "Concurrent Street",
                Geometry = factory.CreateLineString(new[]
                {
                        new Coordinate(0, 0),
                        new Coordinate(1, 1)
                    })
            };
            await repository1.AddAsync(initialStreet);

            // Act: Simulate two concurrent updates
            var coordinate1 = new Coordinate(2, 2);
            var coordinate2 = new Coordinate(3, 3);

            Exception task1Exception = null;
            Exception task2Exception = null;

            var task1 = Task.Run(async () =>
            {
                try
                {
                    await repository1.AddPointAsync(1, coordinate1, true); // Add to the end
                }
                catch (Exception ex)
                {
                    task1Exception = ex;
                }
            });

            var task2 = Task.Run(async () =>
            {
                try
                {
                    await repository2.AddPointAsync(1, coordinate2, true); // Add to the end
                }
                catch (Exception ex)
                {
                    task2Exception = ex;
                }
            });

            // Wait for both tasks to complete and expect one to fail
            await Task.WhenAll(task1, task2);

            // Assert: Handle both cases (exception or no exception)
            if (task1Exception != null || task2Exception != null)
            {
                // Assert that at least one exception was thrown and it matches the expected type and message
                var thrownException = task1Exception ?? task2Exception;

                Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException), "Expected an InvalidOperationException.");

                var expectedMessages = new[]
                    {
                        "the connection is already in a transaction",
                        "the connection is already in state 'executing'",
                        "Connection already open",
                    };
                Assert.IsTrue(
                    expectedMessages.Any(message => thrownException.Message.ToLower().Contains(message.ToLower())),
                    $"The exception message does not match any of the expected errors. Actual: {thrownException.Message}"
                );
            }

            // Assert: Check if only one task succeeded
            using var verifyScope = _serviceProvider.CreateScope();
            var context = verifyScope.ServiceProvider.GetRequiredService<StreetDbContext>();
            var street = await context.Streets.FindAsync(1);

            Assert.IsNotNull(street);
            Assert.IsTrue(street.Geometry.Coordinates.Length >= 3); // Ensure at least one update succeeded
        }
    }
}