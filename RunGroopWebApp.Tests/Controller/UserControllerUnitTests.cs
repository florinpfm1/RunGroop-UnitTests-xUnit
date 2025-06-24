using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;

namespace RunGroopWebApp.Tests.Controller
{
    public class UserControllerUnitTests
    {
        [Fact]
        public async Task Index_ShouldReturnViewWithUserViewModels()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetAllUsers()).ReturnsAsync(new List<AppUser>());

            // Mock dependencies for UserManager
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new UserManager<AppUser>(
                store.Object, 
                null, null, null, null, null, null, null, null);

            var controller = new UserController(mockRepo.Object, userManager, Mock.Of<IPhotoService>());

            var result = await controller.Index();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<List<UserViewModel>>();
        }

        [Fact]
        public async Task Detail_ShouldRedirectToIndexIfUserNull()
        {
            var mockRepo = new Mock<IUserRepository>();
            mockRepo.Setup(r => r.GetUserById(It.IsAny<string>())).ReturnsAsync((AppUser?)null);

            // Create a real UserManager<AppUser> with mocked dependencies
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new UserManager<AppUser>(
                store.Object, null, null, null, null, null, null, null, null);

            var controller = new UserController(mockRepo.Object, userManager, Mock.Of<IPhotoService>());

            var result = await controller.Detail("id");

            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be("Index");
        }
    }
}
