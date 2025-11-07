using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using MongoDB.Driver;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FinanzasPersonales.Tests.Services
{
    public class CategoriaServiceTests
    {
        private readonly Mock<IMongoCollection<Categoria>> _mockCollection;
        private readonly Mock<IMongoDBContext> _mockContext;
        private readonly CategoriaService _service;

        public CategoriaServiceTests()
        {
            _mockCollection = new Mock<IMongoCollection<Categoria>>();
            _mockContext = new Mock<IMongoDBContext>();

            _mockContext.Setup(c => c.Categorias).Returns(_mockCollection.Object);

            _service = new CategoriaService(_mockContext.Object);
        }

        #region Get Tests
        [Fact]
        public async Task GetAsync_ReturnsAllCategorias()
        {
            // Arrange
            var categorias = new List<Categoria>
            {
                new() { Id = "1", Nombre = "Comida", Tipo = "Gasto", UsuarioId = "user1" },
                new() { Id = "2", Nombre = "Transporte", Tipo = "Gasto", UsuarioId = "user1" }
            };

            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.Setup(_ => _.Current).Returns(categorias);
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    It.IsAny<FindOptions<Categoria, Categoria>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Nombre == "Comida");
            Assert.Contains(result, c => c.Nombre == "Transporte");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCategoria_WhenExists()
        {
            // Arrange
            var categoria = new Categoria { Id = "1", Nombre = "Comida", Tipo = "Gasto", UsuarioId = "user1" };

            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria> { categoria });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    It.IsAny<FindOptions<Categoria, Categoria>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Comida", result.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Categoria>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Categoria>());

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    It.IsAny<FindOptions<Categoria, Categoria>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetByIdAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }
        #endregion

            #region GetByUsuario Tests
            [Fact]
            public async Task GetByUsuarioIdAsync_ReturnsCategoriasForUsuario()
            {
                // Arrange
                var usuarioId = "user1";
                var categorias = new List<Categoria>
                {
                    new() { Id = "1", Nombre = "Comida", Tipo = "Gasto", UsuarioId = usuarioId },
                    new() { Id = "2", Nombre = "Transporte", Tipo = "Gasto", UsuarioId = usuarioId }
                };

                var mockCursor = new Mock<IAsyncCursor<Categoria>>();
                mockCursor.Setup(_ => _.Current).Returns(categorias);
                mockCursor
                    .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);

                _mockCollection
                    .Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Categoria>>(),
                        It.IsAny<FindOptions<Categoria, Categoria>>(),
                        It.IsAny<CancellationToken>()
                    ))
                    .ReturnsAsync(mockCursor.Object);

                // Act
                var result = await _service.GetByUsuarioIdAsync(usuarioId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.All(result, c => Assert.Equal(usuarioId, c.UsuarioId));
            }
            #endregion

        #region Create Tests
        [Fact]
        public async Task CreateAsync_InsertsCategoria()
        {
            // Arrange
            var categoria = new Categoria 
            { 
                Id = "1", 
                Nombre = "Salud", 
                Tipo = "Gasto",
                UsuarioId = "user1"
            };

            // Act
            await _service.CreateAsync(categoria);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<Categoria>(c => c.Nombre == "Salud"),
                null,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ValidatesModel()
        {
            // Arrange
            var categoria = new Categoria();  // Missing required fields

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateAsync(categoria));

            Assert.Contains("El nombre de la categoría es obligatorio", exception.Message);
        }

        [Theory]
        [InlineData("InvalidTipo")]
        [InlineData("")]
        public async Task CreateAsync_ValidatesTipo(string tipo)
        {
            // Arrange
            var categoria = new Categoria
            {
                Nombre = "Test",
                Tipo = tipo,
                UsuarioId = "user1"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateAsync(categoria));

            Assert.True(
                exception.Message.Contains("El tipo debe ser 'Ingreso' o 'Gasto'") ||
                exception.Message.Contains("El tipo es obligatorio"),
                "Expected validation message to indicate required or allowed values for Tipo"
            );
        }
        #endregion

        #region Update Tests
        [Fact]
        public async Task UpdateAsync_ValidatesAndUpdates_WhenExists()
        {
            // Arrange
            var categoriaId = "1";
            var categoria = new Categoria
            {
                Id = categoriaId,
                Nombre = "Salud Actualizada",
                Tipo = "Gasto",
                UsuarioId = "user1"
            };

            var filterBuilder = Builders<Categoria>.Filter;
            var filter = filterBuilder.Eq(c => c.Id, categoriaId);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<Categoria>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, categoriaId));

            // Act
            await _service.UpdateAsync(categoriaId, categoria);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.Is<Categoria>(c => c.Nombre == "Salud Actualizada" && c.Tipo == "Gasto"),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidationFails_WhenInvalidModel()
        {
            // Arrange
            var categoriaId = "1";
            var categoria = new Categoria
            {
                Id = categoriaId,
                Nombre = "", // Invalid - required field
                Tipo = "InvalidTipo", // Invalid value
                UsuarioId = "user1"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.UpdateAsync(categoriaId, categoria));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsNotFound_WhenCategoriaDoesNotExist()
        {
            // Arrange
            var categoriaId = "nonexistent";
            var categoria = new Categoria
            {
                Id = categoriaId,
                Nombre = "Test",
                Tipo = "Gasto",
                UsuarioId = "user1"
            };

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<Categoria>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new ReplaceOneResult.Acknowledged(0, 0, null));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(categoriaId, categoria));
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task DeleteAsync_SuccessfullyRemoves_WhenExists()
        {
            // Arrange
            var categoriaId = "1";
            var filterBuilder = Builders<Categoria>.Filter;
            var filter = filterBuilder.Eq(c => c.Id, categoriaId);

            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _service.DeleteAsync(categoriaId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsNotFound_WhenCategoriaDoesNotExist()
        {
            // Arrange
            var categoriaId = "nonexistent";
            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new DeleteResult.Acknowledged(0));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(categoriaId));
        }
        #endregion
    }
}