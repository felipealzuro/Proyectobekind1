using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;
using backend.Controllers;
using backend.Models;

namespace backend.Tests
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task GetProducts_ReturnsOkResult()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Product>>(), null, null))
                .Returns(new List<Product>().AsQueryable());

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "miapp"
            });

            var controller = new ProductsController(mockSettings.Object);

            // Act
            var result = await controller.GetProducts();

            // Assert
            Assert.IsAssignableFrom<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddProduct_WithAdminRole_ReturnsOkResult()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<Product>>();
            mockCollection.Setup(c => c.InsertOneAsync(It.IsAny<Product>(), null, null))
                .Returns(Task.CompletedTask);

            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "miapp"
            });

            var controller = new ProductsController(mockSettings.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = new DefaultHttpRequest
                    {
                        Headers = { { "UserRole", "Admin" } }
                    }
                }
            };

            // Act
            var result = await controller.AddProduct(new Product());

            // Assert
            Assert.IsAssignableFrom<OkResult>(result);
        }

        [Fact]
        public async Task AddProduct_WithoutAdminRole_ReturnsUnauthorizedResult()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<Product>>();
            var mockSettings = new Mock<IOptions<MongoDbSettings>>();
            mockSettings.Setup(s => s.Value).Returns(new MongoDbSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName = "miapp"
            });

            var controller = new ProductsController(mockSettings.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = new DefaultHttpRequest
                    {
                        Headers = { { "UserRole", "Viewer" } }
                    }
                }
            };

            // Act
            var result = await controller.AddProduct(new Product());

            // Assert
            Assert.IsAssignableFrom<UnauthorizedObjectResult>(result);
        }
    }
}