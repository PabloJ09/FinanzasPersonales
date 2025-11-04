using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using MongoDB.Driver;
using Moq;
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

        [Fact]
        public async Task GetAsync_ReturnsAllTransacciones()
        {
            // Arrange
            var transacciones = new List<Transaccion>
            {
                new() { Id = "1", Monto = 100M },
                new() { Id = "2", Monto = 200M }
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
            var transaccion = new Transaccion { Id = "1", Monto = 500M };

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
        }

        [Fact]
        public async Task CreateAsync_InsertsTransaccion()
        {
            // Arrange
            var transaccion = new Transaccion { Id = "1", Monto = 150M };

            // Act
            var result = await _service.CreateAsync(transaccion);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(transaccion, null, default), Times.Once);
            Assert.Equal(150M, result.Monto);
        }

        [Fact]
        public async Task UpdateAsync_DeberiaActualizarTransaccion()
        {
            // Arrange
            var id = "123";
            var transaccionActualizada = new Transaccion
            {
                Id = id,
                Descripcion = "Pago de servicios",
                Monto = 100M,
                Fecha = DateTime.UtcNow,
                CategoriaId = "456"
            };

            _mockCollection
                .Setup(t => t.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Transaccion>>(),
                    transaccionActualizada,
                    It.IsAny<ReplaceOptions>(),
                    default))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, id));

            // Act
            await _service.UpdateAsync(id, transaccionActualizada);

            // Assert
            _mockCollection.Verify(t => t.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                transaccionActualizada,
                It.IsAny<ReplaceOptions>(),
                default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RemovesTransaccion()
        {
            // Act
            await _service.DeleteAsync("1");

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Transaccion>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
