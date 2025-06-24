using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using RunGroopWebApp.Helpers;
using RunGroopWebApp.Services;
using Xunit;

namespace RunGroopWebApp.Tests.Service
{
    public class PhotoServiceUnitTests
    {
        private static IOptions<CloudinarySettings> GetCloudinaryOptions()
        {
            return Options.Create(new CloudinarySettings
            {
                CloudName = "test",
                ApiKey = "key",
                ApiSecret = "secret"
            });
        }

        /*
        [Fact]
        public async Task AddPhotoAsync_ShouldReturnUploadResult()
        {
            // Arrange
            var mockCloudinary = new Mock<Cloudinary>(new Account("test", "key", "secret"));
            var expectedResult = new ImageUploadResult { Url = new System.Uri("http://test.com/image.jpg") };
            mockCloudinary
                .Setup(c => c.UploadAsync(It.IsAny<ImageUploadParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var fileContent = new byte[] { 1, 2, 3 };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(fileContent.Length);
            mockFile.Setup(f => f.FileName).Returns("file.jpg");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fileContent));

            var service = new PhotoService(GetCloudinaryOptions());
            typeof(PhotoService)
                .GetField("_cloundinary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(service, mockCloudinary.Object);

            // Act
            var result = await service.AddPhotoAsync(mockFile.Object);

            // Assert
            result.Should().NotBeNull();
            result.Url.Should().Be(expectedResult.Url);
        }

        [Fact]
        public async Task DeletePhotoAsync_ShouldReturnDeletionResult()
        {
            // Arrange
            var mockCloudinary = new Mock<Cloudinary>(new Account("test", "key", "secret"));
            var expectedResult = new DeletionResult { Result = "ok" };
            mockCloudinary
                .Setup(c => c.DestroyAsync(It.IsAny<DeletionParams>()))
                .ReturnsAsync(expectedResult);

            var service = new PhotoService(GetCloudinaryOptions());
            typeof(PhotoService)
                .GetField("_cloundinary", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(service, mockCloudinary.Object);

            // Act
            var result = await service.DeletePhotoAsync("http://res.cloudinary.com/demo/image/upload/sample.jpg");

            // Assert
            result.Result.Should().Be("ok");
        }
        */
    }
}