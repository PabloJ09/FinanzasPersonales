using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using MongoDB.Driver;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FinanzasPersonales.Tests.Services
{
    public class TransaccionServiceTests
    {
        private readonly Mock<IMongoCollection<Transaccion>> _mockCollection;
        private readonly Mock<IMongoDBContext> _mockContext;
        private readonly TransaccionService _service;

        public TransaccionServiceTests()
        {
            _mockCollection = new Mock<IMongoCollection<Transaccion>>();
            _mockContext = new Mock<IMongoDBContext>();

            _mockContext.Setup(c => c.Transacciones).Returns(_mockCollection.Object);

            _service = new TransaccionService(_mockContext.Object);
        }

        #region Get Tests
        [Fact]
        public async Task GetAsync_ReturnsAllTransacciones()
        {
            // Arrange
            var transacciones = new List<Transaccion>
            {
                new() 
                { 
                    Id = "1", 
                    Tipo = "Gasto", 
                    Monto = 100M, 
                    CategoriaId = "cat1", 
                    UsuarioId = "user1", 
                    Descripcion = "Compras" 
                },
                new() 
                { 
                    Id = "2", 
                    Tipo = "Ingreso", 
                    Monto = 200M, 
                    CategoriaId = "cat2", 
                    UsuarioId = "user1", 
                    Descripcion = "Salario" 
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Transaccion>>();
            mockCursor.Setup(_ => _.Current).Returns(transacciones);
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
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    It.IsAny<FindOptions<Transaccion, Transaccion>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Monto == 100M);
            Assert.Contains(result, t => t.Monto == 200M);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTransaccion_WhenExists()
        {
            // Arrange
            var transaccion = new Transaccion 
            { 
                Id = "1", 
                Tipo = "Gasto", 
                Monto = 500M, 
                CategoriaId = "cat1", 
                UsuarioId = "user1", 
                Descripcion = "Compras" 
            };

            var mockCursor = new Mock<IAsyncCursor<Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Transaccion> { transaccion });

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    It.IsAny<FindOptions<Transaccion, Transaccion>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500M, result.Monto);
            Assert.Equal("Gasto", result.Tipo);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<Transaccion>>();
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);
            mockCursor.Setup(_ => _.Current).Returns(new List<Transaccion>());

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    It.IsAny<FindOptions<Transaccion, Transaccion>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetByIdAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }
        #endregion

        #region Create Tests
        [Fact]
        public async Task CreateAsync_InsertsTransaccion()
        {
            // Arrange
            var transaccion = new Transaccion 
            { 
                Id = "1", 
                Tipo = "Gasto", 
                Monto = 150M, 
                CategoriaId = "cat1", 
                UsuarioId = "user1", 
                Descripcion = "Test" 
            };

            // Act
            await _service.CreateAsync(transaccion);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<Transaccion>(t => t.Monto == 150M),
                null,
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ValidatesModel()
        {
            // Arrange
            var transaccion = new Transaccion();  // Missing required fields

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateAsync(transaccion));

            Assert.Contains("El tipo de transacción es obligatorio", exception.Message);
        }

        [Theory]
        [InlineData("InvalidTipo")]
        [InlineData("")]
        public async Task CreateAsync_ValidatesTipo(string tipo)
        {
            // Arrange
            var transaccion = new Transaccion
            {
                Monto = 100M,
                Tipo = tipo,
                CategoriaId = "cat1",
                UsuarioId = "user1"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateAsync(transaccion));

            Assert.True(
                exception.Message.Contains("El tipo debe ser 'Ingreso' o 'Gasto'") ||
                exception.Message.Contains("El tipo de transacción es obligatorio"),
                "Expected validation message to indicate required or allowed values for Tipo"
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateAsync_ValidatesMonto(decimal monto)
        {
            // Arrange
            var transaccion = new Transaccion
            {
                Tipo = "Gasto",
                Monto = monto,
                CategoriaId = "cat1",
                UsuarioId = "user1"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() =>
                _service.CreateAsync(transaccion));

            Assert.Contains("El monto debe ser mayor que 0", exception.Message);
        }
        #endregion

        #region Update Tests
        [Fact]
        public async Task UpdateAsync_ValidatesAndUpdates_WhenExists()
        {
            // Arrange
            var transaccionId = "1";
            var transaccion = new Transaccion
            {
                Id = transaccionId,
                Tipo = "Gasto",
                Monto = 200M,
                CategoriaId = "cat1",
                UsuarioId = "user1",
                Descripcion = "Updated"
            };

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<Transaccion>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, transaccionId));

            // Act
            await _service.UpdateAsync(transaccionId, transaccion);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.Is<Transaccion>(t => t.Monto == 200M && t.Descripcion == "Updated"),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ValidationFails_WhenInvalidModel()
        {
            // Arrange
            var transaccionId = "1";
            var transaccion = new Transaccion
            {
                Id = transaccionId,
                Tipo = "InvalidTipo",
                Monto = -100M,
                UsuarioId = "user1"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                _service.UpdateAsync(transaccionId, transaccion));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsNotFound_WhenTransaccionDoesNotExist()
        {
            // Arrange
            var transaccionId = "nonexistent";
            var transaccion = new Transaccion
            {
                Id = transaccionId,
                Tipo = "Gasto",
                Monto = 100M,
                CategoriaId = "cat1",
                UsuarioId = "user1"
            };

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<Transaccion>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new ReplaceOneResult.Acknowledged(0, 0, null));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(transaccionId, transaccion));
        }
        #endregion

        #region Delete Tests
        [Fact]
        public async Task DeleteAsync_SuccessfullyRemoves_WhenExists()
        {
            // Arrange
            var transaccionId = "1";
            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new DeleteResult.Acknowledged(1));

            // Act
            await _service.DeleteAsync(transaccionId);

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsNotFound_WhenTransaccionDoesNotExist()
        {
            // Arrange
            var transaccionId = "nonexistent";
            _mockCollection.Setup(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(new DeleteResult.Acknowledged(0));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.DeleteAsync(transaccionId));
        }
        #endregion

        #region GetByUsuario Tests
        [Fact]
            public async Task GetByUsuarioIdAsync_ReturnsUserTransacciones()
        {
            // Arrange
            var usuarioId = "user1";
            var transacciones = new List<Transaccion>
            {
                new() 
                { 
                    Id = "1", 
                    Tipo = "Gasto", 
                    Monto = 100M, 
                    CategoriaId = "cat1", 
                    UsuarioId = usuarioId 
                },
                new() 
                { 
                    Id = "2", 
                    Tipo = "Ingreso", 
                    Monto = 200M, 
                    CategoriaId = "cat2", 
                    UsuarioId = usuarioId 
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Transaccion>>();
            mockCursor.Setup(_ => _.Current).Returns(transacciones);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    It.IsAny<FindOptions<Transaccion, Transaccion>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
                var result = await _service.GetByUsuarioIdAsync(usuarioId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Equal(usuarioId, t.UsuarioId));
        }
        #endregion

        #region GetByCategoria Tests
        [Fact]
            public async Task GetByCategoriaAsync_ReturnsCategoriaTransacciones()
        {
            // Arrange
            var categoriaId = "cat1";
            var transacciones = new List<Transaccion>
            {
                new() 
                { 
                    Id = "1", 
                    Tipo = "Gasto", 
                    Monto = 100M, 
                    CategoriaId = categoriaId, 
                    UsuarioId = "user1" 
                },
                new() 
                { 
                    Id = "2", 
                    Tipo = "Gasto", 
                    Monto = 200M, 
                    CategoriaId = categoriaId, 
                    UsuarioId = "user1" 
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Transaccion>>();
            mockCursor.Setup(_ => _.Current).Returns(transacciones);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    It.IsAny<FindOptions<Transaccion, Transaccion>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
                var result = await _service.GetByCategoriaAsync(categoriaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Equal(categoriaId, t.CategoriaId));
        }
        #endregion
    }
}