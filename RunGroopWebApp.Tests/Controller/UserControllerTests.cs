// File: RunGroopWebApp.Tests/Controller/UserControllerTests.cs
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using CloudinaryDotNet.Actions;

namespace RunGroopWebApp.Tests.Controller
{
    [Trait("Group", "Controller")]
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<IPhotoService> _mockPhotoService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPhotoService = new Mock<IPhotoService>();
            _mockUserManager = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null
            );
            _controller = new UserController(_mockUserRepo.Object, _mockUserManager.Object, _mockPhotoService.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithUserViewModels()
        {
            // Arrange
            var users = new List<AppUser>
            {
                new AppUser { Id = "1", UserName = "user1", City = "City1", State = "ST", Pace = 5, Mileage = 10, ProfileImageUrl = null },
                new AppUser { Id = "2", UserName = "user2", City = "City2", State = "ST", Pace = 6, Mileage = 20, ProfileImageUrl = "img.jpg" }
            };
            _mockUserRepo.Setup(r => r.GetAllUsers()).ReturnsAsync(users);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as List<UserViewModel>;
            model.Should().NotBeNull();
            model.Should().HaveCount(2);
            model![0].UserName.Should().Be("user1");
            model[0].ProfileImageUrl.Should().Be("/img/avatar-male-4.jpg");
            model[1].ProfileImageUrl.Should().Be("img.jpg");
        }

        [Fact]
        public async Task Detail_ReturnsViewResult_WithUserDetailViewModel_WhenUserExists()
        {
            // Arrange
            var user = new AppUser { Id = "1", UserName = "user1", City = "City1", State = "ST", Pace = 5, Mileage = 10, ProfileImageUrl = null };
            _mockUserRepo.Setup(r => r.GetUserById("1")).ReturnsAsync(user);

            // Act
            var result = await _controller.Detail("1");

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as UserDetailViewModel;
            model.Should().NotBeNull();
            model!.UserName.Should().Be("user1");
            model.ProfileImageUrl.Should().Be("/img/avatar-male-4.jpg");
        }

        [Fact]
        public async Task Detail_RedirectsToIndex_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetUserById("badid")).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _controller.Detail("badid");

            // Assert
            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Users");
        }

        [Fact]
        public async Task EditProfile_Get_ReturnsViewResult_WithEditProfileViewModel_WhenUserExists()
        {
            // Arrange
            var user = new AppUser { City = "City", State = "ST", Pace = 5, Mileage = 10, ProfileImageUrl = "img.jpg" };
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            // Act
            var result = await _controller.EditProfile();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as EditProfileViewModel;
            model.Should().NotBeNull();
            model!.City.Should().Be("City");
            model.State.Should().Be("ST");
            model.Pace.Should().Be(5);
            model.Mileage.Should().Be(10);
            model.ProfileImageUrl.Should().Be("img.jpg");
        }

        [Fact]
        public async Task EditProfile_Get_ReturnsErrorView_WhenUserIsNull()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _controller.EditProfile();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task EditProfile_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("City", "Required");
            var editVM = new EditProfileViewModel();

            // Act
            var result = await _controller.EditProfile(editVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("EditProfile");
            viewResult.Model.Should().Be(editVM);
        }

        [Fact]
        public async Task EditProfile_Post_ReturnsErrorView_WhenUserIsNull()
        {
            // Arrange
            var editVM = new EditProfileViewModel();
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((AppUser?)null);

            // Act
            var result = await _controller.EditProfile(editVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task EditProfile_Post_UpdatesProfileImage_WhenImageUploadSucceeds()
        {
            // Arrange
            var user = new AppUser { ProfileImageUrl = "old.jpg" };
            var editVM = new EditProfileViewModel { Image = Mock.Of<IFormFile>() };
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockPhotoService.Setup(s => s.AddPhotoAsync(It.IsAny<IFormFile>())).ReturnsAsync(new ImageUploadResult { Url = new System.Uri("http://img/new.jpg") });
            _mockPhotoService.Setup(s => s.DeletePhotoAsync("old.jpg")).ReturnsAsync(new DeletionResult());
            _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.EditProfile(editVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as EditProfileViewModel;
            model!.ProfileImageUrl.Should().Be("http://img/new.jpg");
            viewResult.ViewName.Should().BeNull(); // Should return default view
        }

        [Fact]
        public async Task EditProfile_Post_UpdatesUserAndRedirects_WhenNoImage()
        {
            // Arrange
            var user = new AppUser { Id = "1" };
            var editVM = new EditProfileViewModel { City = "C", State = "S", Pace = 1, Mileage = 2 };
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.EditProfile(editVM);

            // Assert
            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be("Detail");
            redirect.ControllerName.Should().Be("User");
            redirect.RouteValues!["Id"].Should().Be("1");
            user.City.Should().Be("C");
            user.State.Should().Be("S");
            user.Pace.Should().Be(1);
            user.Mileage.Should().Be(2);
        }
    }
}
