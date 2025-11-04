using FinanzasPersonales.Controllers;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FinanzasPersonales.Tests.Controllers
{
    public class TransaccionesControllerTests
    {
        private readonly Mock<FinanzasPersonales.Database.IMongoDBContext> _mockContext;
        private readonly Mock<MongoDB.Driver.IMongoCollection<FinanzasPersonales.Models.Transaccion>> _mockCollection;
        private readonly TransaccionService _service;
        private readonly TransaccionesController _controller;

        public TransaccionesControllerTests()
        {
            _mockContext = new Mock<FinanzasPersonales.Database.IMongoDBContext>();
            _mockCollection = new Mock<MongoDB.Driver.IMongoCollection<FinanzasPersonales.Models.Transaccion>>();
            _mockContext.Setup(c => c.Transacciones).Returns(_mockCollection.Object);
            _service = new TransaccionService(_mockContext.Object);
            _controller = new TransaccionesController(_service);
        }

        [Fact]
        public async Task Get_ReturnsOkWithTransacciones()
        {
            // Arrange
            var transacciones = new List<Transaccion> { new Transaccion { Id = "1", Monto = 100 } };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(transacciones);
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(transacciones, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenTransaccionDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenTransaccionExists()
        {
            // Arrange
            var transaccion = new FinanzasPersonales.Models.Transaccion { Id = "1", Monto = 100 };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion> { transaccion });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(transaccion, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            // Arrange
            var transaccion = new FinanzasPersonales.Models.Transaccion { Monto = 200 };
            _mockCollection.Setup(c => c.InsertOneAsync(transaccion, null, default)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(transaccion);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(transaccion, createdResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenTransaccionDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Update("1", new FinanzasPersonales.Models.Transaccion());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenTransaccionExists()
        {
            // Arrange
            var transaccion = new FinanzasPersonales.Models.Transaccion { Id = "1" };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion> { transaccion });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                transaccion,
                It.IsAny<MongoDB.Driver.ReplaceOptions>(),
                default)).ReturnsAsync(new MongoDB.Driver.ReplaceOneResult.Acknowledged(1, 1, transaccion.Id));

            // Act
            var result = await _controller.Update("1", transaccion);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenTransaccionDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenTransaccionExists()
        {
            // Arrange
            var transaccion = new FinanzasPersonales.Models.Transaccion { Id = "1" };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<FinanzasPersonales.Models.Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<FinanzasPersonales.Models.Transaccion> { transaccion });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<MongoDB.Driver.FindOptions<FinanzasPersonales.Models.Transaccion, FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<FinanzasPersonales.Models.Transaccion>>(),
                It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(new MongoDB.Driver.DeleteResult.Acknowledged(1));

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
