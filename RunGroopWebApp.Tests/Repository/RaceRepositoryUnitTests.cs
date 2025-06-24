using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using Xunit;

namespace RunGroopWebApp.Tests.Repository
{
    public class RaceRepositoryUnitTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Add_ShouldAddRace()
        {
            var context = GetDbContext(nameof(Add_ShouldAddRace));
            var repo = new RaceRepository(context);
            var race = new Race { Title = "Test Race", Description = "Test Description" };

            var result = repo.Add(race);

            result.Should().BeTrue();
            context.Races.Should().Contain(race);
        }

        [Fact]
        public void Delete_ShouldRemoveRace()
        {
            var context = GetDbContext(nameof(Delete_ShouldRemoveRace));
            var race = new Race { Title = "Test Race", Description = "Test Description" };
            context.Races.Add(race);
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var result = repo.Delete(race);

            result.Should().BeTrue();
            context.Races.Should().NotContain(race);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllRaces()
        {
            var context = GetDbContext(nameof(GetAll_ShouldReturnAllRaces));
            context.Races.AddRange(new Race { Title = "A", Description = "Test Description1" }, new Race { Title = "B", Description = "Test Description2" });
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var races = await repo.GetAll();

            races.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRace()
        {
            var context = GetDbContext(nameof(GetByIdAsync_ShouldReturnCorrectRace));
            var address = new Address { Id = 1, Street = "First Street", City = "NYC", State = "NY" };
            context.Addresses.Add(address);
            context.SaveChanges();
            var race = new Race { AddressId = 1, Address = address, Description = "Test Description1", Title = "Race1" };
            context.Races.Add(race);
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var result = await repo.GetByIdAsync(race.Id);

            result.Should().NotBeNull();
            result.Title.Should().Be("Race1");
        }

        [Fact]
        public void Update_ShouldUpdateRace()
        {
            var context = GetDbContext(nameof(Update_ShouldUpdateRace));
            var address = new Address { Id = 1, Street = "First Street", City = "NYC", State = "NY" };
            context.Addresses.Add(address);
            context.SaveChanges();
            var race = new Race { AddressId = 1, Address = address, Description = "Test Description1", Title = "Old" };
            context.Races.Add(race);
            context.SaveChanges();
            var repo = new RaceRepository(context);
            race.Title = "New";

            var result = repo.Update(race);

            result.Should().BeTrue();
            context.Races.First().Title.Should().Be("New");
        }

        [Fact]
        public async Task GetCountAsync_ShouldReturnRaceCount()
        {
            var context = GetDbContext(nameof(GetCountAsync_ShouldReturnRaceCount));
            var address1 = new Address { Id = 1, Street = "First Street", City = "NYC", State = "NY" };
            var address2 = new Address { Id = 2, Street = "Second Street", City = "NYC", State = "NY" };
            context.Addresses.AddRange(address1, address2);
            context.Races.AddRange(new Race { AddressId = 1, Address = address1, Description = "Test Description1", Title = "Race1" }, new Race { AddressId = 2, Address = address2, Description = "Test Description2", Title = "Race2" });
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var count = await repo.GetCountAsync();

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetAllRacesByCity_ShouldReturnRaces()
        {
            var context = GetDbContext(nameof(GetAllRacesByCity_ShouldReturnRaces));
            context.Addresses.Add(new Address { Id = 1, Street = "First Street", City = "NYC", State = "NY" });
            context.SaveChanges();
            context.Races.Add(new Race { AddressId = 1, Address = context.Addresses.First(), Description = "Test Description1", Title = "Race1" });
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var result = await repo.GetAllRacesByCity("NYC");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCountByCategoryAsync_ShouldReturnCorrectCount()
        {
            var context = GetDbContext(nameof(GetCountByCategoryAsync_ShouldReturnCorrectCount));
            var address1 = new Address { Id = 1, Street = "First Street", City = "NYC", State = "NY" };
            var address2 = new Address { Id = 2, Street = "Second Street", City = "NYC", State = "NY" };
            var address3 = new Address { Id = 3, Street = "Third Street", City = "NYC", State = "NY" };
            context.Addresses.AddRange(address1, address2, address3);
            context.Races.AddRange(
                new Race { AddressId = 1, Address = address1, Description = "Test Description1", Title = "Race1", RaceCategory = RaceCategory.Marathon },
                new Race { AddressId = 2, Address = address2, Description = "Test Description2", Title = "Race2", RaceCategory = RaceCategory.HalfMarathon },
                new Race { AddressId = 3, Address = address3, Description = "Test Description3", Title = "Race3", RaceCategory = RaceCategory.Marathon });
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var count = await repo.GetCountByCategoryAsync(RaceCategory.Marathon);

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetSliceAsync_ShouldReturnCorrectSlice()
        {
            var context = GetDbContext(nameof(GetSliceAsync_ShouldReturnCorrectSlice));
            for (int i = 1; i <= 10; i++)
            {
                context.Races.Add(new Race { AddressId = i, Address = new Address {Id = i, Street = $"Street{i}", City = $"City{i}", State = $"State{i}" }, Description = "Text", Title = $"Race{i}" });
            }
            context.SaveChanges();
            var repo = new RaceRepository(context);

            var result = await repo.GetSliceAsync(2, 3);

            result.Should().HaveCount(3);
            result.Select(r => r.Title).Should().Contain(new[] { "Race3", "Race4", "Race5" });
        }

        [Fact]
        public async Task GetRacesByCategoryAndSliceAsync_ShouldReturnCorrectSlice()
        {
            var context = GetDbContext(nameof(GetRacesByCategoryAndSliceAsync_ShouldReturnCorrectSlice));
            for (int i = 1; i <= 10; i++)
            {
                context.Races.Add(
                    new Race { 
                        AddressId = i, 
                        Address = new Address { Id = i, Street = $"Street{i}", City = $"City{i}", State = $"State{i}" }, 
                        Description = "Text", 
                        Title = $"Race{i}",
                        RaceCategory = RaceCategory.Marathon
                    });
            }
            context.Races.Add(
                new Race { 
                    AddressId = 11,
                    Address = new Address { Id = 11, Street = "Street11", City = "City11", State = "State11" },
                    Description = "Text",
                    Title = "Other", 
                    RaceCategory = RaceCategory.HalfMarathon });

            context.SaveChanges();
            var repo = new RaceRepository(context);

            var result = await repo.GetRacesByCategoryAndSliceAsync(RaceCategory.Marathon, 2, 3);

            result.Should().HaveCount(3);
            result.All(r => r.RaceCategory == RaceCategory.Marathon).Should().BeTrue();
        }
    }
}
