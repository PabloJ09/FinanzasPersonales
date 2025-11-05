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
    public class CategoriasControllerTests
    {
        private readonly Mock<FinanzasPersonales.Database.IMongoDBContext> _mockContext;
        private readonly Mock<MongoDB.Driver.IMongoCollection<Categoria>> _mockCollection;
        private readonly CategoriaService _service;
        private readonly CategoriasController _controller;

        public CategoriasControllerTests()
        {
            _mockContext = new Mock<FinanzasPersonales.Database.IMongoDBContext>();
            _mockCollection = new Mock<MongoDB.Driver.IMongoCollection<Categoria>>();
            _mockContext.Setup(c => c.Categorias).Returns(_mockCollection.Object);
            _service = new CategoriaService(_mockContext.Object);
            _controller = new CategoriasController(_service);
        }

        [Fact]
        public async Task Get_ReturnsOkWithCategorias()
        {
            // Arrange
            var categorias = new List<Categoria> { new Categoria { Id = "1", Nombre = "Comida" } };
            // Mock el cursor para simular la respuesta de Mongo
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(categorias);
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categorias, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenCategoriaDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenCategoriaExists()
        {
            // Arrange
            var categoria = new Categoria { Id = "1", Nombre = "Comida" };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { categoria });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(categoria, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            // Arrange
            var categoria = new Categoria { Nombre = "Nueva", Tipo = "Gasto", UsuarioId = "user1" };
            _mockCollection.Setup(c => c.InsertOneAsync(It.IsAny<Categoria>(), null, default)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(categoria);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var created = Assert.IsType<Categoria>(createdResult.Value);
            Assert.Equal(categoria.Nombre, created.Nombre);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenCategoriaDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Update("1", new Categoria());

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenCategoriaExists()
        {
            // Arrange
            var categoria = new Categoria { Id = "1", Nombre = "Comida", Tipo = "Gasto", UsuarioId = "user1" };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { categoria });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                categoria,
                It.IsAny<MongoDB.Driver.ReplaceOptions>(),
                default)).ReturnsAsync(new MongoDB.Driver.ReplaceOneResult.Acknowledged(1, 1, categoria.Id));

            // Act
            var result = await _controller.Update("1", categoria);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCategoriaDoesNotExist()
        {
            // Arrange
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria>());
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenCategoriaExists()
        {
            // Arrange
            var categoria = new Categoria { Id = "1" };
            var mockCursor = new Mock<MongoDB.Driver.IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<System.Threading.CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { categoria });
            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<MongoDB.Driver.FindOptions<Categoria, Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<MongoDB.Driver.FilterDefinition<Categoria>>(),
                It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(new MongoDB.Driver.DeleteResult.Acknowledged(1));

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
