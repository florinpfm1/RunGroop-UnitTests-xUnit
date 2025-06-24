using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RunGroopWebApp.Data;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using Xunit;

namespace RunGroopWebApp.Tests.Repository
{
    public class DashboardRepositoryUnitTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static IHttpContextAccessor MockHttpContextAccessor(string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));
            var context = new DefaultHttpContext { User = user };
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            return accessor.Object;
        }

        [Fact]
        public async Task GetAllUserClubs_ShouldReturnClubsForCurrentUser()
        {
            var context = GetDbContext(nameof(GetAllUserClubs_ShouldReturnClubsForCurrentUser));
            var userId = "user1";
            var user = new AppUser { Id = userId };
            var club1 = new Club { Id = 1, AppUser = user };
            var club2 = new Club { Id = 2, AppUser = new AppUser { Id = "user2" } };
            context.Clubs.AddRange(club1, club2);
            context.SaveChanges();
            var accessor = MockHttpContextAccessor(userId);
            var repo = new DashboardRepository(context, accessor);

            var result = await repo.GetAllUserClubs();

            result.Should().ContainSingle(c => c.Id == 1);
        }

        
        [Fact]
        public async Task GetAllUserRaces_ShouldReturnRacesForCurrentUser()
        {
            var context = GetDbContext(nameof(GetAllUserRaces_ShouldReturnRacesForCurrentUser));
            var address1 = new Address { Id = 1, Street = "123 Main St", City = "Testville", State = "TS", ZipCode = 12345 };
            var address2 = new Address { Id = 2, Street = "456 Side St", City = "Sampletown", State = "ST", ZipCode = 67890 };
            context.Addresses.AddRange(address1, address2);
            context.SaveChanges();

            var user1 = new AppUser { Id = "user1" };
            var user2 = new AppUser { Id = "user2" };
            var race1 = new Race
            {
                Id = 1,
                Title = "Race One",
                Description = "First race description",
                Image = "race1.jpg",
                StartTime = new System.DateTime(2023, 5, 1, 8, 0, 0),
                EntryFee = 50,
                Website = "https://raceone.com",
                Twitter = "@raceone",
                Facebook = "raceonefb",
                Contact = "contact@raceone.com",
                AddressId = address1.Id,
                Address = address1,
                RaceCategory = RaceCategory.Marathon,
                AppUserId = "user1",
                AppUser = user1
            };
            var race2 = new Race
            {
                Id = 2,
                Title = "Race Two",
                Description = "Second race description",
                Image = "race2.jpg",
                StartTime = new System.DateTime(2023, 6, 1, 9, 0, 0),
                EntryFee = 75,
                Website = "https://racetwo.com",
                Twitter = "@racetwo",
                Facebook = "racetwofb",
                Contact = "contact@racetwo.com",
                AddressId = address2.Id,
                Address = address2,
                RaceCategory = RaceCategory.HalfMarathon,
                AppUserId = "user2",
                AppUser = user2
            };
            context.Races.AddRange(race1, race2);
            context.SaveChanges();
            var accessor = MockHttpContextAccessor(user1.Id);
            var repo = new DashboardRepository(context, accessor);

            var result = await repo.GetAllUserRaces();

            result.Should().ContainSingle(r => r.Id == 1);
        }
        

        [Fact]
        public async Task GetUserById_ShouldReturnUser()
        {
            var context = GetDbContext(nameof(GetUserById_ShouldReturnUser));
            var user = new AppUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(user);
            context.SaveChanges();
            var accessor = Mock.Of<IHttpContextAccessor>();
            var repo = new DashboardRepository(context, accessor);

            var result = await repo.GetUserById("user1");

            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByIdNoTracking_ShouldReturnUserWithoutTracking()
        {
            var context = GetDbContext(nameof(GetByIdNoTracking_ShouldReturnUserWithoutTracking));
            var user = new AppUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(user);
            context.SaveChanges();
            var accessor = Mock.Of<IHttpContextAccessor>();
            var repo = new DashboardRepository(context, accessor);

            var result = await repo.GetByIdNoTracking("user1");

            result.Should().NotBeNull();
            result.UserName.Should().Be("testuser");
        }

        
        [Fact]
        public void Update_ShouldUpdateUser()
        {
            var context = GetDbContext(nameof(Update_ShouldUpdateUser));
            var user = new AppUser { Id = "user1", Pace = 10 };
            context.Users.Add(user);
            context.SaveChanges();
            var accessor = Mock.Of<IHttpContextAccessor>();
            var repo = new DashboardRepository(context, accessor);
            user.Pace = 8;

            var result = repo.Update(user);

            result.Should().BeTrue();
            context.Users.First().Pace.Should().Be(8);
        }
        

        [Fact]
        public void Save_ShouldReturnTrueIfChangesSaved()
        {
            var context = GetDbContext(nameof(Save_ShouldReturnTrueIfChangesSaved));
            var user = new AppUser { Id = "user1" };
            context.Users.Add(user);
            var accessor = Mock.Of<IHttpContextAccessor>();
            var repo = new DashboardRepository(context, accessor);

            var result = repo.Save();

            result.Should().BeTrue();
        }
    }
}
