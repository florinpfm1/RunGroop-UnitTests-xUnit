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
    public class ClubRepositoryTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public void Add_ShouldAddClub()
        {
            var context = GetDbContext(nameof(Add_ShouldAddClub));
            var repo = new ClubRepository(context);
            var club = new Club { Title = "Test Club" };

            var result = repo.Add(club);

            result.Should().BeTrue();
            context.Clubs.Should().Contain(club);
        }

        [Fact]
        public void Delete_ShouldRemoveClub()
        {
            var context = GetDbContext(nameof(Delete_ShouldRemoveClub));
            var club = new Club { Title = "ToDelete" };
            context.Clubs.Add(club);
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var result = repo.Delete(club);

            result.Should().BeTrue();
            context.Clubs.Should().NotContain(club);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllClubs()
        {
            var context = GetDbContext(nameof(GetAll_ShouldReturnAllClubs));
            context.Clubs.AddRange(new Club { Title = "A" }, new Club { Title = "B" });
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var clubs = await repo.GetAll();

            clubs.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectClub()
        {
            var context = GetDbContext(nameof(GetByIdAsync_ShouldReturnCorrectClub));
            var club = new Club { Title = "FindMe" };
            context.Clubs.Add(club);
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var result = await repo.GetByIdAsync(club.Id);

            result.Should().NotBeNull();
            result.Title.Should().Be("FindMe");
        }

        [Fact]
        public void Update_ShouldUpdateClub()
        {
            var context = GetDbContext(nameof(Update_ShouldUpdateClub));
            var club = new Club { Title = "Old" };
            context.Clubs.Add(club);
            context.SaveChanges();
            var repo = new ClubRepository(context);
            club.Title = "New";

            var result = repo.Update(club);

            result.Should().BeTrue();
            context.Clubs.First().Title.Should().Be("New");
        }

        [Fact]
        public async Task GetCountAsync_ShouldReturnClubCount()
        {
            var context = GetDbContext(nameof(GetCountAsync_ShouldReturnClubCount));
            context.Clubs.AddRange(new Club(), new Club());
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var count = await repo.GetCountAsync();

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetClubsByState_ShouldReturnClubs()
        {
            var context = GetDbContext(nameof(GetClubsByState_ShouldReturnClubs));
            context.Addresses.Add(new Address { Id = 1, Street = "Main Street", City = "NYC", State = "NY", ZipCode = 10001 });
            context.SaveChanges(); // Save the address first
            context.Clubs.Add(new Club { AddressId = 1, Address = context.Addresses.First() });
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var result = await repo.GetClubsByState("NY");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetClubByCity_ShouldReturnClubs()
        {
            var context = GetDbContext(nameof(GetClubByCity_ShouldReturnClubs));
            context.Addresses.Add(new Address { Id = 1, Street = "Main Street", City = "NYC", State = "NY", ZipCode = 10001 });
            context.SaveChanges(); // Save the address first
            context.Clubs.Add(new Club { AddressId = 1, Address = context.Addresses.First() });
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var result = await repo.GetClubByCity("NYC");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllCitiesByState_ShouldReturnCities()
        {
            var context = GetDbContext(nameof(GetAllCitiesByState_ShouldReturnCities));
            context.Cities.Add(new City { CityName = "LA", StateCode = "CA", County = "Los Angeles" });
            context.Cities.Add(new City { CityName = "NYC", StateCode = "NY", County = "New York" });
            context.SaveChanges();
            var repo = new ClubRepository(context);

            var result = await repo.GetAllCitiesByState("CA");

            result.Should().ContainSingle(c => c.CityName == "LA");
        }
    }
}
