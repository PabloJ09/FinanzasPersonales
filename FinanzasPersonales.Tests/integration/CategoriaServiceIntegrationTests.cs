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
    public class CategoriaServiceIntegrationTests : IntegrationTestBase
    {
        private CategoriaService _service = null!;
        private IRepository<Categoria> _repository = null!;
        private string _usuarioId = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = new MongoRepository<Categoria>(DbContext.Categorias);
            IValidator<Categoria> validator = new CategoriaValidator();
            _service = new CategoriaService(_repository, validator);

            // Generate a valid ObjectId for the test usuario
            _usuarioId = ObjectId.GenerateNewId().ToString();
        }

        [Fact]
        public async Task CreateAsync_Persists_Categoria_To_Database()
        {
            // Arrange
            var categoria = new Categoria
            {
                Nombre = "Comida",
                Tipo = "Gasto",
                UsuarioId = _usuarioId
            };

            // Act
            var created = await _service.CreateAsync(categoria);

            // Assert
            Assert.NotNull(created);
            Assert.NotNull(created.Id);
            var retrieved = await _service.GetByIdAsync(created.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Comida", retrieved.Nombre);
        }

        [Fact]
        public async Task GetAllAsync_Returns_All_Categorias_From_Database()
        {
            // Arrange
            var cat1 = new Categoria { Nombre = "Comida", Tipo = "Gasto", UsuarioId = _usuarioId };
            var cat2 = new Categoria { Nombre = "Salario", Tipo = "Ingreso", UsuarioId = _usuarioId };

            await _service.CreateAsync(cat1);
            await _service.CreateAsync(cat2);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2, "Should have at least 2 categorias");
        }

        [Fact]
        public async Task UpdateAsync_Modifies_Existing_Categoria()
        {
            // Arrange
            var original = await _service.CreateAsync(new Categoria
            {
                Nombre = "Viejo",
                Tipo = "Gasto",
                UsuarioId = _usuarioId
            });

            var updated = new Categoria
            {
                Nombre = "Nuevo",
                Tipo = "Ingreso",
                UsuarioId = _usuarioId
            };

            // Act
            var result = await _service.UpdateAsync(original.Id!, updated);

            // Assert
            Assert.Equal("Nuevo", result.Nombre);
            Assert.Equal("Ingreso", result.Tipo);

            var verified = await _service.GetByIdAsync(original.Id!);
            Assert.Equal("Nuevo", verified!.Nombre);
        }

        [Fact]
        public async Task DeleteAsync_Removes_Categoria_From_Database()
        {
            // Arrange
            var created = await _service.CreateAsync(new Categoria
            {
                Nombre = "ToDelete",
                Tipo = "Gasto",
                UsuarioId = _usuarioId
            });

            // Act
            await _service.DeleteAsync(created.Id!);

            // Assert
            var retrieved = await _service.GetByIdAsync(created.Id!);
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task GetByUsuarioIdAsync_Returns_Only_User_Categorias()
        {
            // Arrange
            var usuarioId2 = ObjectId.GenerateNewId().ToString();
            await _service.CreateAsync(new Categoria { Nombre = "Cat1", Tipo = "Gasto", UsuarioId = _usuarioId });
            await _service.CreateAsync(new Categoria { Nombre = "Cat2", Tipo = "Gasto", UsuarioId = _usuarioId });
            await _service.CreateAsync(new Categoria { Nombre = "Cat3", Tipo = "Gasto", UsuarioId = usuarioId2 });

            // Act
            var user1Cats = await _service.GetByUsuarioIdAsync(_usuarioId);

            // Assert
            Assert.NotNull(user1Cats);
            Assert.Equal(2, user1Cats.Count);
            Assert.All(user1Cats, c => Assert.Equal(_usuarioId, c.UsuarioId));
        }

        [Fact]
        public async Task UpdatePartialAsync_Updates_Only_Provided_Fields()
        {
            // Arrange
            var original = await _service.CreateAsync(new Categoria
            {
                Nombre = "Original",
                Tipo = "Gasto",
                UsuarioId = _usuarioId
            });

            var partial = new Categoria { Nombre = "Updated" };

            // Act
            var result = await _service.UpdatePartialAsync(original.Id!, partial);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated", result.Nombre);
            Assert.Equal("Gasto", result.Tipo); // No cambió
        }
    }
}
