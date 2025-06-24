using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RunGroopWebApp.Tests.Controller
{
    public class AccountControllerUnitTests
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
        public void Login_Get_ShouldReturnViewWithModel()
        {
            var controller = new AccountController(null, null, null, null);

            var result = controller.Login();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<LoginViewModel>();
        }

        [Fact]
        public async Task Login_Post_InvalidModelState_ShouldReturnView()
        {
            var controller = new AccountController(null, null, null, null);
            controller.ModelState.AddModelError("EmailAddress", "Required");
            var model = new LoginViewModel();

            var result = await controller.Login(model);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task Login_Post_ValidCredentials_ShouldRedirectToRaceIndex()
        {
            var userManager = GetUserManagerMock();
            var signInManager = GetSignInManagerMock(userManager.Object);
            var user = new AppUser { Email = "test@test.com" };
            userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            userManager.Setup(u => u.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
            signInManager.Setup(s => s.PasswordSignInAsync(user, It.IsAny<string>(), false, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            var controller = new AccountController(userManager.Object, signInManager.Object, null, null);
            var model = new LoginViewModel { EmailAddress = "test@test.com", Password = "pass" };

            var result = await controller.Login(model);

            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Race");
        }

        [Fact]
        public async Task Register_Get_ShouldReturnViewWithModel()
        {
            var controller = new AccountController(null, null, null, null);

            var result = controller.Register();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<RegisterViewModel>();
        }

        [Fact]
        public async Task Register_Post_InvalidModelState_ShouldReturnView()
        {
            var controller = new AccountController(null, null, null, null);
            controller.ModelState.AddModelError("EmailAddress", "Required");
            var model = new RegisterViewModel();

            var result = await controller.Register(model);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task Register_Post_EmailExists_ShouldReturnViewWithError()
        {
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(new AppUser());
            var controller = new AccountController(userManager.Object, null, null, null);

            // Add this block to set up TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;

            var model = new RegisterViewModel { EmailAddress = "exists@test.com", Password = "pass", ConfirmPassword = "pass" };

            var result = await controller.Register(model);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            controller.TempData["Error"].Should().NotBeNull();
        }

        [Fact]
        public async Task Register_Post_Success_ShouldRedirectToRaceIndex()
        {
            var userManager = GetUserManagerMock();
            userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser)null);
            userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(u => u.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            var controller = new AccountController(userManager.Object, null, null, null);
            var model = new RegisterViewModel { EmailAddress = "new@test.com", Password = "pass", ConfirmPassword = "pass" };

            var result = await controller.Register(model);

            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Race");
        }

        [Fact]
        public async Task Logout_ShouldRedirectToRaceIndex()
        {
            var signInManager = GetSignInManagerMock(GetUserManagerMock().Object);
            signInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);
            var controller = new AccountController(null, signInManager.Object, null, null);

            var result = await controller.Logout();

            var redirect = result as RedirectToActionResult;
            redirect.Should().NotBeNull();
            redirect.ActionName.Should().Be("Index");
            redirect.ControllerName.Should().Be("Race");
        }
    }
}
