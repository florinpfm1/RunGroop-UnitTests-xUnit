using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;

namespace RunGroopWebApp.Tests.Controllers
{
    [Trait("Group", "Controller")]
    public class DashboardControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithDashboardViewModel()
        {
            // Arrange
            var mockDashboardRepo = new Mock<IDashboardRepository>();
            var mockPhotoService = new Mock<IPhotoService>();

            var races = new List<Race> { new Race { Id = 1, Title = "Race1" } };
            var clubs = new List<Club> { new Club { Id = 1, Title = "Club1" } };

            mockDashboardRepo.Setup(r => r.GetAllUserRaces()).ReturnsAsync(races);
            mockDashboardRepo.Setup(r => r.GetAllUserClubs()).ReturnsAsync(clubs);

            var controller = new DashboardController(mockDashboardRepo.Object, mockPhotoService.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DashboardViewModel>(viewResult.Model);
            Assert.Equal(races, model.Races);
            Assert.Equal(clubs, model.Clubs);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WhenNoRacesOrClubs()
        {
            // Arrange
            var mockDashboardRepo = new Mock<IDashboardRepository>();
            var mockPhotoService = new Mock<IPhotoService>();

            mockDashboardRepo.Setup(r => r.GetAllUserRaces()).ReturnsAsync(new List<Race>());
            mockDashboardRepo.Setup(r => r.GetAllUserClubs()).ReturnsAsync(new List<Club>());

            var controller = new DashboardController(mockDashboardRepo.Object, mockPhotoService.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DashboardViewModel>(viewResult.Model);
            Assert.Empty(model.Races);
            Assert.Empty(model.Clubs);
        }
    }
}
