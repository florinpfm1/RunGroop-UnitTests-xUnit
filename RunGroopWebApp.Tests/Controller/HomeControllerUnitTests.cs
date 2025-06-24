using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;

namespace RunGroopWebApp.Tests.Controller
{
    public class HomeControllerUnitTests
    {
        private Mock<UserManager<AppUser>> GetUserManagerMock()
        {
            var store = new Mock<IUserStore<AppUser>>();
            return new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private Mock<SignInManager<AppUser>> GetSignInManagerMock(UserManager<AppUser> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();
            return new Mock<SignInManager<AppUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithHomeViewModel()
        {
            var mockLogger = new Mock<ILogger<HomeController>>();
            var mockClubRepo = new Mock<IClubRepository>();
            mockClubRepo.Setup(r => r.GetClubByCity(It.IsAny<string>())).ReturnsAsync(new System.Collections.Generic.List<Club>());
            var mockUserManager = GetUserManagerMock();
            var mockSignInManager = GetSignInManagerMock(mockUserManager.Object);
            var mockLocationService = new Mock<ILocationService>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var controller = new HomeController(mockLogger.Object, mockClubRepo.Object, mockUserManager.Object, mockSignInManager.Object, mockLocationService.Object, mockConfig.Object);

            var result = await controller.Index();

            result.Should().BeOfType<ViewResult>();
        }
    }
}
