using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
    public class ClubControllerUnitTests
    {
        [Fact]
        public async Task Index_ShouldReturnViewWithModel()
        {
            var mockRepo = new Mock<IClubRepository>();
            mockRepo.Setup(r => r.GetSliceAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Club>());
            mockRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(0);
            var controller = new ClubController(mockRepo.Object, Mock.Of<IPhotoService>());

            var result = await controller.Index();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<IndexClubViewModel>();
        }

        [Fact]
        public async Task ListClubsByState_ShouldReturnViewWithModel()
        {
            var mockRepo = new Mock<IClubRepository>();
            mockRepo.Setup(r => r.GetClubsByState(It.IsAny<string>())).ReturnsAsync(new List<Club>());
            var controller = new ClubController(mockRepo.Object, Mock.Of<IPhotoService>());

            var result = await controller.ListClubsByState("CALIFORNIA");

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<ListClubByStateViewModel>();
        }

        [Fact]
        public async Task DetailClub_ShouldReturnNotFoundIfNull()
        {
            var mockRepo = new Mock<IClubRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Club)null);
            var controller = new ClubController(mockRepo.Object, Mock.Of<IPhotoService>());

            var result = await controller.DetailClub(1, "test");

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
