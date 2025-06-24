using System.Threading.Tasks;
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
    public class RaceControllerUnitTests
    {
        [Fact]
        public async Task Index_ShouldReturnViewWithModel()
        {
            var mockRepo = new Mock<IRaceRepository>();
            mockRepo.Setup(r => r.GetSliceAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new System.Collections.Generic.List<Race>());
            mockRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(0);
            var controller = new RaceController(mockRepo.Object, Mock.Of<IPhotoService>(), Mock.Of<IHttpContextAccessor>());

            var result = await controller.Index();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<IndexRaceViewModel>();
        }

        [Fact]
        public async Task DetailRace_ShouldReturnNotFoundIfNull()
        {
            var mockRepo = new Mock<IRaceRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Race)null);
            var controller = new RaceController(mockRepo.Object, Mock.Of<IPhotoService>(), Mock.Of<IHttpContextAccessor>());

            var result = await controller.DetailRace(1, "test");

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
