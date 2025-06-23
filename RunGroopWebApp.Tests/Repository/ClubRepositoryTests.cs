using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RunGroopWebApp.Data;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RunGroopWebApp.Tests.Repository
{
    [Trait("Group", "Repository")]
    public class ClubRepositoryTests
    {
        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        private Club CreateClub(string title = "Test Club", string city = "Charlotte", string state = "NC", ClubCategory category = ClubCategory.City)
        {
            return new Club
            {
                Title = title,
                Description = "Description",
                Image = "image.jpg",
                ClubCategory = category,
                Address = new Address
                {
                    Street = "123 Main St",
                    City = city,
                    State = state,
                    ZipCode = 12345
                }
            };
        }

        [Fact]
        public async Task Add_AddsClubToDatabase()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();

            var result = repo.Add(club);

            result.Should().BeTrue();
            db.Clubs.Should().Contain(club);
        }

        [Fact]
        public async Task Delete_RemovesClubFromDatabase()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();
            db.Clubs.Add(club);
            db.SaveChanges();

            var result = repo.Delete(club);

            result.Should().BeTrue();
            db.Clubs.Should().NotContain(club);
        }

        [Fact]
        public async Task GetAll_ReturnsAllClubs()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club1 = CreateClub("Club1");
            var club2 = CreateClub("Club2");
            db.Clubs.AddRange(club1, club2);
            db.SaveChanges();

            var result = await repo.GetAll();

            result.Should().Contain(new[] { club1, club2 });
        }

        [Fact]
        public async Task GetSliceAsync_ReturnsCorrectSlice()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            for (int i = 0; i < 10; i++)
                db.Clubs.Add(CreateClub($"Club{i}"));
            db.SaveChanges();

            var result = await repo.GetSliceAsync(2, 3);

            result.Count().Should().Be(3);
            result.First().Title.Should().Be("Club2");
        }

        [Fact]
        public async Task GetClubsByCategoryAndSliceAsync_ReturnsCorrectClubs()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            db.Clubs.Add(CreateClub("Road", category: ClubCategory.RoadRunner));
            db.Clubs.Add(CreateClub("Trail", category: ClubCategory.Trail));
            db.Clubs.Add(CreateClub("City", category: ClubCategory.City));
            db.SaveChanges();

            var result = await repo.GetClubsByCategoryAndSliceAsync(ClubCategory.Trail, 0, 2);

            result.Should().ContainSingle(c => c.ClubCategory == ClubCategory.Trail);
        }

        [Fact]
        public async Task GetCountByCategoryAsync_ReturnsCorrectCount()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            db.Clubs.Add(CreateClub("Road", category: ClubCategory.RoadRunner));
            db.Clubs.Add(CreateClub("Trail", category: ClubCategory.Trail));
            db.Clubs.Add(CreateClub("City", category: ClubCategory.City));
            db.SaveChanges();

            var count = await repo.GetCountByCategoryAsync(ClubCategory.City);

            count.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectClub()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();
            db.Clubs.Add(club);
            db.SaveChanges();

            var result = await repo.GetByIdAsync(club.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(club.Id);
        }

        [Fact]
        public async Task GetByIdAsyncNoTracking_ReturnsCorrectClubWithoutTracking()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();
            db.Clubs.Add(club);
            db.SaveChanges();

            var result = await repo.GetByIdAsyncNoTracking(club.Id);

            result.Should().NotBeNull();
            result.Id.Should().Be(club.Id);
        }

        [Fact]
        public async Task GetClubByCity_ReturnsClubsByCity()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            db.Clubs.Add(CreateClub("A", city: "Charlotte"));
            db.Clubs.Add(CreateClub("B", city: "Raleigh"));
            db.SaveChanges();

            var result = await repo.GetClubByCity("Charlotte");

            result.Should().OnlyContain(c => c.Address.City.Contains("Charlotte"));
        }

        [Fact]
        public async Task Save_PersistsChanges()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();
            db.Clubs.Add(club);
            var changed = repo.Save();
            changed.Should().BeTrue();
        }

        [Fact]
        public async Task Update_UpdatesClub()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            var club = CreateClub();
            db.Clubs.Add(club);
            db.SaveChanges();
            club.Title = "Updated Title";

            var result = repo.Update(club);

            result.Should().BeTrue();
            db.Clubs.First().Title.Should().Be("Updated Title");
        }

        [Fact]
        public async Task GetCountAsync_ReturnsTotalCount()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            db.Clubs.Add(CreateClub("A"));
            db.Clubs.Add(CreateClub("B"));
            db.SaveChanges();

            var count = await repo.GetCountAsync();

            count.Should().Be(2);
        }

        [Fact]
        public async Task GetClubsByState_ReturnsClubsByState()
        {
            var db = await GetDbContext();
            var repo = new ClubRepository(db);
            db.Clubs.Add(CreateClub("A", state: "NC"));
            db.Clubs.Add(CreateClub("B", state: "SC"));
            db.SaveChanges();

            var result = await repo.GetClubsByState("NC");

            result.Should().OnlyContain(c => c.Address.State.Contains("NC"));
        }

        [Fact]
        public async Task GetAllStates_ReturnsAllStates()
        {
            var db = await GetDbContext();
            db.States.Add(new State { StateName = "North Carolina", StateCode = "NC" });
            db.States.Add(new State { StateName = "South Carolina", StateCode = "SC" });
            db.SaveChanges();
            var repo = new ClubRepository(db);

            var result = await repo.GetAllStates();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllCitiesByState_ReturnsCitiesByStateCode()
        {
            var db = await GetDbContext();
            db.Cities.Add(new City { CityName = "Charlotte", StateCode = "NC", Zip = 12345, Latitude = 1.1, Longitude = 2.2, County = "CountyA" });
            db.Cities.Add(new City { CityName = "Raleigh", StateCode = "NC", Zip = 12346, Latitude = 3.3, Longitude = 4.4, County = "CountyB" });
            db.Cities.Add(new City { CityName = "Columbia", StateCode = "SC", Zip = 22345, Latitude = 5.5, Longitude = 6.6, County = "CountyC" });
            db.SaveChanges();
            var repo = new ClubRepository(db);

            var result = await repo.GetAllCitiesByState("NC");

            result.Should().OnlyContain(c => c.StateCode.Contains("NC"));
        }
    }
}
