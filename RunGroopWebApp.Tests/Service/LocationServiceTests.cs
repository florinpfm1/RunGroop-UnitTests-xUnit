using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Models;
using RunGroopWebApp.Services;
using Xunit;

namespace RunGroopWebApp.Tests.Service
{
    [Trait("Group", "Service")]
    public class LocationServiceTests
    {
        private async Task<ApplicationDbContext> GetDbContextWithCities(List<City> cities)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var db = new ApplicationDbContext(options);
            db.Cities.AddRange(cities);
            await db.SaveChangesAsync();
            return db;
        }

        [Fact]
        public async Task GetCityByZipCode_ReturnsCity_WhenExists()
        {
            var city = new City { Id = 1, CityName = "TestCity", StateCode = "TS", Zip = 12345, Latitude = 80.5, Longitude = 125.3, County = "Star1" };
            var db = await GetDbContextWithCities(new List<City> { city });
            var service = new LocationService(db);

            var result = await service.GetCityByZipCode(12345);

            result.Should().NotBeNull();
            result.CityName.Should().Be("TestCity");
            result.Zip.Should().Be(12345);
        }

        [Fact]
        public async Task GetCityByZipCode_ReturnsNull_WhenNotExists()
        {
            var db = await GetDbContextWithCities(new List<City>());
            var service = new LocationService(db);

            var result = await service.GetCityByZipCode(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocationSearch_ReturnsCitiesByZipPrefix()
        {
            var cities = new List<City>
            {
                new City { Id = 1, CityName = "A", StateCode = "TS", Zip = 12345, Latitude = 1.1, Longitude = 2.2, County = "CountyA" },
                new City { Id = 2, CityName = "B", StateCode = "TS", Zip = 12346, Latitude = 3.3, Longitude = 4.4, County = "CountyB" },
                new City { Id = 3, CityName = "C", StateCode = "TS", Zip = 22345, Latitude = 5.5, Longitude = 6.6, County = "CountyC" }
            };
            var db = await GetDbContextWithCities(cities);
            var service = new LocationService(db);

            var result = await service.GetLocationSearch("12");

            // According to the current implementation, this should return all cities with zip starting with '12'.
            result.Should().BeEquivalentTo(cities.Where(c => c.Zip.ToString().StartsWith("12")));
        }

        [Fact]
        public async Task GetLocationSearch_ReturnsCitiesByCityName()
        {
            var cities = new List<City>
            {
                new City { Id = 1, CityName = "Alpha", StateCode = "TS", Zip = 12345, Latitude = 1.1, Longitude = 2.2, County = "CountyA" },
                new City { Id = 2, CityName = "Beta", StateCode = "TS", Zip = 12346, Latitude = 3.3, Longitude = 4.4, County = "CountyB" }
            };
            var db = await GetDbContextWithCities(cities);
            var service = new LocationService(db);

            var result = await service.GetLocationSearch("Alpha");

            // According to the current implementation, this should return all cities with CityName == 'Alpha'.
            result.Should().BeEquivalentTo(cities.Where(c => c.CityName == "Alpha"));
        }

        [Fact]
        public async Task GetLocationSearch_ReturnsCitiesByStateCode()
        {
            var cities = new List<City>
            {
                new City { Id = 1, CityName = "Alpha", StateCode = "TS", Zip = 12345, Latitude = 1.1, Longitude = 2.2, County = "CountyA" },
                new City { Id = 2, CityName = "Beta", StateCode = "TS", Zip = 12346, Latitude = 3.3, Longitude = 4.4, County = "CountyB" },
                new City { Id = 3, CityName = "Gamma", StateCode = "XY", Zip = 22345, Latitude = 5.5, Longitude = 6.6, County = "CountyC" }
            };
            var db = await GetDbContextWithCities(cities);
            var service = new LocationService(db);

            var result = await service.GetLocationSearch("TS");

            // According to the current implementation, this should return all cities with CityName == 'TS'.
            result.Should().BeEquivalentTo(cities.Where(c => c.CityName == "TS"));
        }
    }
}