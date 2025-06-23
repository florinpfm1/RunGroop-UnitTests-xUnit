// File: RunGroopWebApp.Tests/Controller/HomeControllerTests.cs
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using System.Threading.Tasks;
using FluentAssertions;
using System.Collections.Generic;
using RunGroopWebApp.Data;

namespace RunGroopWebApp.Tests.Controller
{
    [Trait("Group", "Controller")]
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IClubRepository> _mockClubRepo;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
        private readonly Mock<ILocationService> _mockLocationService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockClubRepo = new Mock<IClubRepository>();
            _mockUserManager = new Mock<UserManager<AppUser>>(
                Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null
            );
            _mockSignInManager = new Mock<SignInManager<AppUser>>(
                _mockUserManager.Object,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
                null, null, null, null
            );
            _mockLocationService = new Mock<ILocationService>();
            _mockConfig = new Mock<IConfiguration>();
            _controller = new HomeController(
                _mockLogger.Object,
                _mockClubRepo.Object,
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockLocationService.Object,
                _mockConfig.Object
            );
        }

        [Fact]
        public async Task Index_Post_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            var homeVM = new HomeViewModel { Register = new HomeUserCreateViewModel() };
            _controller.ModelState.AddModelError("Register.Email", "Required");

            // Act
            var result = await _controller.Index(homeVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(homeVM);
        }

        [Fact]
        public async Task Index_Post_ReturnsView_WhenEmailAlreadyExists()
        {
            // Arrange
            var register = new HomeUserCreateViewModel { Email = "test@test.com" };
            var homeVM = new HomeViewModel { Register = register };
            _mockUserManager.Setup(m => m.FindByEmailAsync(register.Email)).ReturnsAsync(new AppUser());

            // Act
            var result = await _controller.Index(homeVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            _controller.ModelState["Register.Email"].Errors.Should().NotBeEmpty();
            viewResult!.Model.Should().Be(homeVM);
        }

        [Fact]
        public async Task Index_Post_ReturnsView_WhenZipCodeNotFound()
        {
            // Arrange
            var register = new HomeUserCreateViewModel { Email = "test@test.com", ZipCode = 12345 };
            var homeVM = new HomeViewModel { Register = register };
            _mockUserManager.Setup(m => m.FindByEmailAsync(register.Email)).ReturnsAsync((AppUser?)null);
            _mockLocationService.Setup(s => s.GetCityByZipCode(register.ZipCode.Value)).ReturnsAsync((City?)null);

            // Act
            var result = await _controller.Index(homeVM);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            _controller.ModelState["Register.ZipCode"].Errors.Should().NotBeEmpty();
            viewResult!.Model.Should().Be(homeVM);
        }

        
        [Fact]
        public async Task Index_Post_CreatesUserAndRedirects_WhenSuccess()
        {
            // Arrange
            var register = new HomeUserCreateViewModel
            {
                Email = "test@test.com",
                UserName = "user",
                Password = "Password1!",
                ZipCode = 12345
            };
            var homeVM = new HomeViewModel { Register = register };
            var city = new City { StateCode = "IN", CityName = "Indianapolis", Zip = 12345, Latitude = 0, Longitude = 0, County = "" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(register.Email)).ReturnsAsync((AppUser?)null);
            _mockLocationService.Setup(s => s.GetCityByZipCode(register.ZipCode.Value)).ReturnsAsync(city);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), register.Password)).ReturnsAsync(IdentityResult.Success);
            _mockSignInManager.Setup(m => m.SignInAsync(It.IsAny<AppUser>(), false, null)).Returns(Task.CompletedTask);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), UserRoles.User)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Index(homeVM);

            // Assert
            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Club");
        }
        

        
        [Fact]
        public async Task Index_Post_Redirects_WhenUserCreationFails()
        {
            // Arrange
            var register = new HomeUserCreateViewModel
            {
                Email = "test@test.com",
                UserName = "user",
                Password = "Password1!",
                ZipCode = 12345
            };
            var homeVM = new HomeViewModel { Register = register };
            var city = new City { StateCode = "IN", CityName = "Indianapolis", Zip = 12345, Latitude = 0, Longitude = 0, County = "" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(register.Email)).ReturnsAsync((AppUser?)null);
            _mockLocationService.Setup(s => s.GetCityByZipCode(register.ZipCode.Value)).ReturnsAsync(city);
            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), register.Password)).ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _controller.Index(homeVM);

            // Assert
            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect!.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Club");
        }
        
    }
}
