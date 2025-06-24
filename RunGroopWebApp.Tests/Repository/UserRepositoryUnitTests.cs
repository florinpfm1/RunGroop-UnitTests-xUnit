using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using Xunit;

namespace RunGroopWebApp.Tests.Repository
{
    public class UserRepositoryUnitTests
    {
        private ApplicationDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers()
        {
            var context = GetDbContext(nameof(GetAllUsers_ShouldReturnAllUsers));
            context.Users.AddRange(new AppUser { Id = "1", UserName = "A" }, new AppUser { Id = "2", UserName = "B" });
            context.SaveChanges();
            var repo = new UserRepository(context);

            var users = await repo.GetAllUsers();

            users.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnCorrectUser()
        {
            var context = GetDbContext(nameof(GetUserById_ShouldReturnCorrectUser));
            var user = new AppUser { Id = "1", UserName = "TestUser" };
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new UserRepository(context);

            var result = await repo.GetUserById("1");

            result.Should().NotBeNull();
            result.UserName.Should().Be("TestUser");
        }

        /*
        [Fact]
        public void Update_ShouldUpdateUser()
        {
            var context = GetDbContext(nameof(Update_ShouldUpdateUser));
            var user = new AppUser { Id = "1", Pace = 10 };
            context.Users.Add(user);
            context.SaveChanges();
            var repo = new UserRepository(context);
            user.Pace = 15;

            var result = repo.Update(user);

            result.Should().BeTrue();
            context.Users.First().Pace.Should().Be(15);
        }
        */

        [Fact]
        public void Save_ShouldReturnTrueIfChangesSaved()
        {
            var context = GetDbContext(nameof(Save_ShouldReturnTrueIfChangesSaved));
            var user = new AppUser { Id = "1" };
            context.Users.Add(user);
            var repo = new UserRepository(context);

            var result = repo.Save();

            result.Should().BeTrue();
        }
    }
}
