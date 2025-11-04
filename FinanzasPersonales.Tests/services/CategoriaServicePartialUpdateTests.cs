using FinanzasPersonales.Services;
using FinanzasPersonales.Models;
using FinanzasPersonales.Database;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FinanzasPersonales.Tests.Services
{
    public class CategoriaServicePartialUpdateTests
    {
        private readonly Mock<IMongoCollection<Categoria>> _mockCollection;
        private readonly Mock<IMongoDBContext> _mockContext;
        private readonly CategoriaService _service;

        public CategoriaServicePartialUpdateTests()
        {
            _mockCollection = new Mock<IMongoCollection<Categoria>>();
            _mockContext = new Mock<IMongoDBContext>();
            _mockContext.Setup(c => c.Categorias).Returns(_mockCollection.Object!);
            _service = new CategoriaService(_mockContext.Object);
        }

        [Fact]
        public async Task UpdatePartialAsync_UpdatesOnlyProvidedFields()
        {
            // Arrange
            var id = "abc";
            var existing = new Categoria { Id = id, Nombre = "Viejo", Tipo = "Gasto", UsuarioId = "u1" };
            var partial = new Categoria { Nombre = "Nuevo", Tipo = string.Empty, UsuarioId = "u2" };

            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { existing });

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<FindOptions<Categoria, Categoria>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<Categoria>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, id));

            // Act
            var result = await _service.UpdatePartialAsync(id, partial);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nuevo", result.Nombre);
            Assert.Equal("Gasto", result.Tipo); // No cambia
            Assert.Equal("u2", result.UsuarioId);

            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.Is<Categoria>(cat => cat.Nombre == "Nuevo" && cat.Tipo == "Gasto" && cat.UsuarioId == "u2"),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePartialAsync_UpdatesTipo_WhenProvided()
        {
            // Arrange
            var id = "xyz";
            var existing = new Categoria { Id = id, Nombre = "Comida", Tipo = "Gasto", UsuarioId = "u1" };
            var partial = new Categoria { Tipo = "Ingreso" }; // se actualiza este campo

            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { existing });

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<FindOptions<Categoria, Categoria>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<Categoria>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, id));

            // Act
            var result = await _service.UpdatePartialAsync(id, partial);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Comida", result.Nombre); // no cambia
            Assert.Equal("Ingreso", result.Tipo);  // sí cambia
            Assert.Equal("u1", result.UsuarioId);  // no cambia

            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.Is<Categoria>(cat => cat.Tipo == "Ingreso" && cat.Nombre == "Comida" && cat.UsuarioId == "u1"),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePartialAsync_ReturnsNull_WhenCategoriaNotFound()
        {
            // Arrange
            var id = "notfound";
            var partial = new Categoria { Nombre = "Nuevo" };

            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria>());

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<FindOptions<Categoria, Categoria>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.UpdatePartialAsync(id, partial);

            // Assert
            Assert.Null(result);
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<Categoria>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
