using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;

namespace RunGroopWebApp.Tests.Controller
{
    [Trait("Group", "Controller")]
    public class RaceControllerTests
    {
        private readonly Mock<IRaceRepository> _raceRepoMock;
        private readonly Mock<IPhotoService> _photoServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly RaceController _controller;

        public RaceControllerTests()
        {
            _raceRepoMock = new Mock<IRaceRepository>();
            _photoServiceMock = new Mock<IPhotoService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _controller = new RaceController(_raceRepoMock.Object, _photoServiceMock.Object, _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenPageOrPageSizeInvalid()
        {
            var result = await _controller.Index(page: 0, pageSize: 0);
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_ForAllCategories()
        {
            var races = new List<Race> { new Race { Id = 1, Title = "Race1" } };
            _raceRepoMock.Setup(r => r.GetSliceAsync(0, 6)).ReturnsAsync(races);
            _raceRepoMock.Setup(r => r.GetCountAsync()).ReturnsAsync(races.Count);

            var result = await _controller.Index();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<IndexRaceViewModel>();
            var model = viewResult.Model as IndexRaceViewModel;
            model!.Races.Should().BeEquivalentTo(races);
            model.Page.Should().Be(1);
            model.PageSize.Should().Be(6);
            model.TotalRaces.Should().Be(races.Count);
            model.TotalPages.Should().Be(1);
            model.Category.Should().Be(-1);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_ForSpecificCategory()
        {
            var races = new List<Race> { new Race { Id = 2, Title = "Race2" } };
            _raceRepoMock.Setup(r => r.GetRacesByCategoryAndSliceAsync(RaceCategory.Marathon, 0, 6)).ReturnsAsync(races);
            _raceRepoMock.Setup(r => r.GetCountByCategoryAsync(RaceCategory.Marathon)).ReturnsAsync(races.Count);

            var result = await _controller.Index((int)RaceCategory.Marathon);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as IndexRaceViewModel;
            model!.Races.Should().BeEquivalentTo(races);
            model.Category.Should().Be((int)RaceCategory.Marathon);
        }

        [Fact]
        public async Task DetailRace_ReturnsNotFound_WhenRaceNotFound()
        {
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Race)null);
            var result = await _controller.DetailRace(1, "any");
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DetailRace_ReturnsView_WhenRaceFound()
        {
            var race = new Race { Id = 1, Title = "Race1" };
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(race);
            var result = await _controller.DetailRace(1, "any");
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(race);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithUserId()
        {
            var userId = "user123";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = user };
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(context);

            var result = _controller.Create();
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as CreateRaceViewModel;
            model!.AppUserId.Should().Be(userId);
        }

        [Fact]
        public async Task Create_Post_RedirectsOnSuccess()
        {
            var raceVM = new CreateRaceViewModel
            {
                Title = "Race1",
                Description = "Desc",
                Address = new Address { Street = "S", City = "C", State = "ST" },
                Image = Mock.Of<IFormFile>(),
                RaceCategory = RaceCategory.FiveK,
                AppUserId = "user1"
            };
            _photoServiceMock.Setup(p => p.AddPhotoAsync(raceVM.Image)).ReturnsAsync(new ImageUploadResult { Url = new System.Uri("http://img") });
            _raceRepoMock.Setup(r => r.Add(It.IsAny<Race>())).Returns(true);

            var result = await _controller.Create(raceVM);
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_Post_ReturnsView_WhenModelStateInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var raceVM = new CreateRaceViewModel();
            var result = await _controller.Create(raceVM);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(raceVM);
        }

        [Fact]
        public async Task Edit_Get_ReturnsView_WhenRaceFound()
        {
            var race = new Race { Id = 1, Title = "Race1", Description = "Desc", AddressId = 2, Address = new Address(), Image = "img", RaceCategory = RaceCategory.FiveK };
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(race);
            var result = await _controller.Edit(1);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult!.Model as EditRaceViewModel;
            model!.Title.Should().Be(race.Title);
            model.AddressId.Should().Be(race.AddressId);
        }

        [Fact]
        public async Task Edit_Get_ReturnsErrorView_WhenRaceNotFound()
        {
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Race)null);
            var result = await _controller.Edit(1);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task Edit_Post_RedirectsOnSuccess()
        {
            var raceVM = new EditRaceViewModel { Title = "Race1", Description = "Desc", Image = Mock.Of<IFormFile>(), AddressId = 2, Address = new Address(), RaceCategory = RaceCategory.FiveK };
            var userRace = new Race { Id = 1, Image = "oldimg" };
            _raceRepoMock.Setup(r => r.GetByIdAsyncNoTracking(1)).ReturnsAsync(userRace);
            _photoServiceMock.Setup(p => p.AddPhotoAsync(raceVM.Image)).ReturnsAsync(new ImageUploadResult { Url = new System.Uri("http://img") });
            _photoServiceMock.Setup(p => p.DeletePhotoAsync(userRace.Image)).ReturnsAsync(new DeletionResult());
            _raceRepoMock.Setup(r => r.Update(It.IsAny<Race>())).Returns(true);

            var result = await _controller.Edit(1, raceVM);
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenModelStateInvalid()
        {
            var raceVM = new EditRaceViewModel();
            _controller.ModelState.AddModelError("Title", "Required");
            var result = await _controller.Edit(1, raceVM);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(raceVM);
        }

        [Fact]
        public async Task Edit_Post_ReturnsErrorView_WhenRaceNotFound()
        {
            var raceVM = new EditRaceViewModel();
            _raceRepoMock.Setup(r => r.GetByIdAsyncNoTracking(1)).ReturnsAsync((Race)null);
            var result = await _controller.Edit(1, raceVM);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task Edit_Post_ReturnsView_WhenPhotoUploadFails()
        {
            var raceVM = new EditRaceViewModel { Image = Mock.Of<IFormFile>() };
            var userRace = new Race { Id = 1 };
            _raceRepoMock.Setup(r => r.GetByIdAsyncNoTracking(1)).ReturnsAsync(userRace);
            _photoServiceMock.Setup(p => p.AddPhotoAsync(raceVM.Image)).ReturnsAsync(new ImageUploadResult { Error = new Error() });
            var result = await _controller.Edit(1, raceVM);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(raceVM);
        }

        [Fact]
        public async Task Delete_Get_ReturnsView_WhenRaceFound()
        {
            var race = new Race { Id = 1 };
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(race);
            var result = await _controller.Delete(1);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().Be(race);
        }

        [Fact]
        public async Task Delete_Get_ReturnsErrorView_WhenRaceNotFound()
        {
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Race)null);
            var result = await _controller.Delete(1);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }

        [Fact]
        public async Task DeleteClub_Post_RedirectsOnSuccess()
        {
            var race = new Race { Id = 1, Image = "img" };
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(race);
            _photoServiceMock.Setup(p => p.DeletePhotoAsync(race.Image)).ReturnsAsync(new DeletionResult());
            _raceRepoMock.Setup(r => r.Delete(race)).Returns(true);
            var result = await _controller.DeleteClub(1);
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task DeleteClub_Post_ReturnsErrorView_WhenRaceNotFound()
        {
            _raceRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Race)null);
            var result = await _controller.DeleteClub(1);
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.ViewName.Should().Be("Error");
        }
    }
}
