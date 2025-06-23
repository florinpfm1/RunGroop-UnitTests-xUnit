using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using FluentAssertions;

namespace RunGroopWebApp.Tests.Controller
{
    [Trait("Group", "Controller")]
    public class ClubControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithIndexClubViewModel_ForAllCategories()
        {
            // Arrange
            var mockClubRepo = new Mock<IClubRepository>();
            var mockPhotoService = new Mock<IPhotoService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var clubs = new List<Club>
            {
                new Club { Id = 1, Title = "Test Club 1" },
                new Club { Id = 2, Title = "Test Club 2" }
            };

            mockClubRepo.Setup(r => r.GetSliceAsync(0, 6)).ReturnsAsync(clubs);
            mockClubRepo.Setup(r => r.GetCountAsync()).ReturnsAsync(clubs.Count);

            var controller = new ClubController(mockClubRepo.Object, mockPhotoService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<IndexClubViewModel>();

            var model = viewResult.Model as IndexClubViewModel;
            model!.Clubs.Should().BeEquivalentTo(clubs);
            model.Page.Should().Be(1);
            model.PageSize.Should().Be(6);
            model.TotalClubs.Should().Be(clubs.Count);
            model.TotalPages.Should().Be(1);
            model.Category.Should().Be(-1);
        }

        [Fact]
        public async Task ListClubsByState_ReturnsView_WithClubsAndState_WhenClubsExist()
        {
            // Arrange
            var mockClubRepo = new Mock<IClubRepository>();
            var mockPhotoService = new Mock<IPhotoService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var state = "INDIANA"; // Use full state name to match StateConverter
            var clubs = new List<Club>
            {
                new Club { Id = 1, Title = "Test Club" }
            };

            mockClubRepo.Setup(r => r.GetClubsByState("IN")).ReturnsAsync(clubs); // StateConverter.GetStateByName("INDIANA") => State.IN => "IN"

            var controller = new ClubController(mockClubRepo.Object, mockPhotoService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await controller.ListClubsByState(state);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<ListClubByStateViewModel>();

            var model = viewResult.Model as ListClubByStateViewModel;
            model!.Clubs.Should().BeEquivalentTo(clubs);
            model.State.Should().Be(state);
            model.NoClubWarning.Should().BeFalse();
        }

        [Fact]
        public async Task ListClubsByState_ReturnsView_WithNoClubWarning_WhenNoClubsExist()
        {
            // Arrange
            var mockClubRepo = new Mock<IClubRepository>();
            var mockPhotoService = new Mock<IPhotoService>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var state = "INDIANA"; // Use full state name to match StateConverter
            var clubs = new List<Club>();

            mockClubRepo.Setup(r => r.GetClubsByState("IN")).ReturnsAsync(clubs); // StateConverter.GetStateByName("INDIANA") => State.IN => "IN"

            var controller = new ClubController(mockClubRepo.Object, mockPhotoService.Object, mockHttpContextAccessor.Object);

            // Act
            var result = await controller.ListClubsByState(state);

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeOfType<ListClubByStateViewModel>();

            var model = viewResult.Model as ListClubByStateViewModel;
            model!.Clubs.Should().BeEmpty();
            model.NoClubWarning.Should().BeTrue();
            model.State.Should().BeNull();
        }
    }
}

