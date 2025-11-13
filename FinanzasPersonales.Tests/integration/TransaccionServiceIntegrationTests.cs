using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.Validators;
using FluentValidation;
using MongoDB.Bson;
using Xunit;

namespace FinanzasPersonales.Tests.Integration
{
    [Collection("Integration Tests")]
    public class TransaccionServiceIntegrationTests : IntegrationTestBase
    {
        private TransaccionService _service = null!;
        private IRepository<Transaccion> _repository = null!;
        private string _usuarioId = null!;
        private string _categoriaId = null!;
        private string _categoriaId2 = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = new MongoRepository<Transaccion>(DbContext.Transacciones);
            IValidator<Transaccion> validator = new TransaccionValidator();
            _service = new TransaccionService(_repository, validator);

            // Generate valid ObjectIds for usuario and categorias
            _usuarioId = ObjectId.GenerateNewId().ToString();
            _categoriaId = ObjectId.GenerateNewId().ToString();
            _categoriaId2 = ObjectId.GenerateNewId().ToString();
        }

        [Fact]
        public async Task CreateAsync_Persists_Transaccion_To_Database()
        {
            // Arrange
            var transaccion = new Transaccion
            {
                Tipo = "Gasto",
                Monto = 50.50M,
                Descripcion = "Compra de comida",
                CategoriaId = _categoriaId,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            };

            // Act
            var created = await _service.CreateAsync(transaccion);

            // Assert
            Assert.NotNull(created);
            Assert.NotNull(created.Id);
            var retrieved = await _service.GetByIdAsync(created.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(50.50M, retrieved.Monto);
        }

        [Fact]
        public async Task GetAllAsync_Returns_All_Transacciones()
        {
            // Arrange
            await _service.CreateAsync(new Transaccion
            {
                Tipo = "Gasto",
                Monto = 100M,
                CategoriaId = _categoriaId,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            });

            await _service.CreateAsync(new Transaccion
            {
                Tipo = "Ingreso",
                Monto = 1000M,
                CategoriaId = _categoriaId2,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            });

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2, "Should have at least 2 transacciones");
        }

        [Fact]
        public async Task GetByUsuarioIdAsync_Returns_Only_User_Transacciones()
        {
            // Arrange
            var usuarioId2 = ObjectId.GenerateNewId().ToString();
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 10M, CategoriaId = _categoriaId, UsuarioId = _usuarioId, Fecha = DateTime.UtcNow });
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 20M, CategoriaId = _categoriaId, UsuarioId = _usuarioId, Fecha = DateTime.UtcNow });
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 30M, CategoriaId = _categoriaId, UsuarioId = usuarioId2, Fecha = DateTime.UtcNow });

            // Act
            var user1Trans = await _service.GetByUsuarioIdAsync(_usuarioId);

            // Assert
            Assert.NotNull(user1Trans);
            Assert.Equal(2, user1Trans.Count);
            Assert.All(user1Trans, t => Assert.Equal(_usuarioId, t.UsuarioId));
        }

        [Fact]
        public async Task GetByCategoriaAsync_Returns_Only_Categoria_Transacciones()
        {
            // Arrange
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 10M, CategoriaId = _categoriaId, UsuarioId = _usuarioId, Fecha = DateTime.UtcNow });
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 20M, CategoriaId = _categoriaId, UsuarioId = _usuarioId, Fecha = DateTime.UtcNow });
            await _service.CreateAsync(new Transaccion { Tipo = "Gasto", Monto = 30M, CategoriaId = _categoriaId2, UsuarioId = _usuarioId, Fecha = DateTime.UtcNow });

            // Act
            var cat1Trans = await _service.GetByCategoriaAsync(_categoriaId);

            // Assert
            Assert.NotNull(cat1Trans);
            Assert.Equal(2, cat1Trans.Count);
            Assert.All(cat1Trans, t => Assert.Equal(_categoriaId, t.CategoriaId));
        }

        [Fact]
        public async Task UpdateAsync_Modifies_Existing_Transaccion()
        {
            // Arrange
            var original = await _service.CreateAsync(new Transaccion
            {
                Tipo = "Gasto",
                Monto = 50M,
                Descripcion = "Original",
                CategoriaId = _categoriaId,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            });

            var updated = new Transaccion
            {
                Tipo = "Ingreso",
                Monto = 100M,
                Descripcion = "Updated",
                CategoriaId = _categoriaId2,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            };

            // Act
            var result = await _service.UpdateAsync(original.Id!, updated);

            // Assert
            Assert.Equal("Ingreso", result.Tipo);
            Assert.Equal(100M, result.Monto);
        }

        [Fact]
        public async Task DeleteAsync_Removes_Transaccion()
        {
            // Arrange
            var created = await _service.CreateAsync(new Transaccion
            {
                Tipo = "Gasto",
                Monto = 50M,
                CategoriaId = _categoriaId,
                UsuarioId = _usuarioId,
                Fecha = DateTime.UtcNow
            });

            // Act
            await _service.DeleteAsync(created.Id!);

            // Assert
            var retrieved = await _service.GetByIdAsync(created.Id!);
            Assert.Null(retrieved);
        }
    }
}
