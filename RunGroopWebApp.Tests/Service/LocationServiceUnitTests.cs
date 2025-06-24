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
    public class LocationServiceUnitTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCityByZipCode_ShouldReturnCorrectCity()
        {
            var context = GetDbContext(nameof(GetCityByZipCode_ShouldReturnCorrectCity));
            var city = new City { Id = 1, CityName = "TestCity", Zip = 12345, County = "TestCounty", StateCode = "TC" };
            context.Cities.Add(city);
            context.SaveChanges();
            var service = new LocationService(context);

            var result = await service.GetCityByZipCode(12345);

            result.Should().NotBeNull();
            result.CityName.Should().Be("TestCity");
        }

        [Fact]
        public async Task GetLocationSearch_ShouldReturnCitiesByZipPrefix()
        {
            var context = GetDbContext(nameof(GetLocationSearch_ShouldReturnCitiesByZipPrefix));
            context.Cities.Add(new City { Id = 1, CityName = "A", Zip = 12345, County = "TestCounty", StateCode = "TC" });
            context.Cities.Add(new City { Id = 2, CityName = "B", Zip = 12346, County = "TestCounty", StateCode = "TC" });
            context.Cities.Add(new City { Id = 3, CityName = "C", Zip = 22345, County = "TestCounty", StateCode = "TC" });
            context.SaveChanges();
            var service = new LocationService(context);

            var result = await service.GetLocationSearch("123");

            result.Should().HaveCount(2);
            result.All(c => c.Zip.ToString().StartsWith("123")).Should().BeTrue();
        }

        /*
        [Fact]
        public async Task GetLocationSearch_ShouldReturnCitiesByCityName()
        {
            var context = GetDbContext(nameof(GetLocationSearch_ShouldReturnCitiesByCityName));
            context.Cities.Add(new City { Id = 1, CityName = "Springfield", Zip = 11111, County = "County1", StateCode = "C1", Latitude = 55, Longitude = 95 });
            context.Cities.Add(new City { Id = 2, CityName = "Springfield", Zip = 22222, County = "County1", StateCode = "C1", Latitude = 55, Longitude = 95 });
            context.Cities.Add(new City { Id = 3, CityName = "Other", Zip = 33333, County = "County3", StateCode = "C3", Latitude = 55, Longitude = 95 });
            context.SaveChanges();
            var service = new LocationService(context);

            var result = await service.GetLocationSearch("Springfield");

            result.Should().OnlyContain(c => c.CityName == "Springfield");
        }

        [Fact]
        public async Task GetLocationSearch_ShouldReturnCitiesByStateCode()
        {
            var context = GetDbContext(nameof(GetLocationSearch_ShouldReturnCitiesByStateCode));
            context.Cities.Add(new City { Id = 1, CityName = "A", StateCode = "CA", Zip = 11111 });
            context.Cities.Add(new City { Id = 2, CityName = "B", StateCode = "CA", Zip = 22222 });
            context.Cities.Add(new City { Id = 3, CityName = "C", StateCode = "NY", Zip = 33333 });
            context.SaveChanges();
            var service = new LocationService(context);

            var result = await service.GetLocationSearch("CA");

            result.Should().OnlyContain(c => c.StateCode == "CA");
        }
        */
    }
}
