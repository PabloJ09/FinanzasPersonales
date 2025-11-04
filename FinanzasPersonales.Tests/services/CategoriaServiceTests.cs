using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using MongoDB.Driver;
using Moq;
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

        [Fact]
        public async Task GetAsync_ReturnsAllCategorias()
        {
            // Arrange
            var categorias = new List<Categoria>
            {
                new() { Id = "1", Nombre = "Comida" },
                new() { Id = "2", Nombre = "Transporte" }
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
            var categoria = new Categoria { Id = "1", Nombre = "Comida" };


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
        public async Task CreateAsync_InsertsCategoria()
        {
            // Arrange
            var categoria = new Categoria { Id = "1", Nombre = "Salud" };

            // Act
            var result = await _service.CreateAsync(categoria);

            // Assert
            _mockCollection.Verify(c => c.InsertOneAsync(categoria, null, default), Times.Once);
            Assert.Equal("Salud", result.Nombre);
        }

        [Fact]
        public async Task UpdateAsync_DeberiaActualizarCategoria()
        {
            // Arrange
            var id = "123";
            var categoriaActualizada = new Categoria { Id = id, Nombre = "Entretenimiento", Tipo = "Gasto" };

            _mockCollection
                .Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<Categoria>>(),
                    categoriaActualizada,
                    It.IsAny<ReplaceOptions>(),
                    default))
                .ReturnsAsync(new ReplaceOneResult.Acknowledged(1, 1, id));

            // Act
            await _service.UpdateAsync(id, categoriaActualizada);
            // Assert
            _mockCollection.Verify(c => c.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                categoriaActualizada,
                It.IsAny<ReplaceOptions>(),
                default), Times.Once);
        }        

 

        [Fact]
        public async Task DeleteAsync_RemovesCategoria()
        {
            // Act
            await _service.DeleteAsync("1");

            // Assert
            _mockCollection.Verify(c => c.DeleteOneAsync(
                It.IsAny<FilterDefinition<Categoria>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
