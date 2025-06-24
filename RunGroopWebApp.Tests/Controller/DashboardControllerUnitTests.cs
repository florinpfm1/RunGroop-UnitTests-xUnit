using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using Xunit;

namespace RunGroopWebApp.Tests.Controller
{
    public class DashboardControllerUnitTests
    {
        [Fact]
        public async Task Index_ShouldReturnViewWithDashboardViewModel()
        {
            var mockRepo = new Mock<IDashboardRepository>();
            mockRepo.Setup(r => r.GetAllUserRaces()).ReturnsAsync(new List<Race>());
            mockRepo.Setup(r => r.GetAllUserClubs()).ReturnsAsync(new List<Club>());
            var controller = new DashboardController(mockRepo.Object, Mock.Of<IPhotoService>());

            var result = await controller.Index();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.Model.Should().BeOfType<DashboardViewModel>();
        }
    }
}
