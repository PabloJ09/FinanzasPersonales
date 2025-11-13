using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace FinanzasPersonales.Tests.Integration
{
    [Collection("Integration Tests")]
    public class UsuarioServiceIntegrationTests : IntegrationTestBase
    {
        private UsuarioService _service = null!;
        private IRepository<Usuario> _repository = null!;
        private Mock<IConfiguration> _configMock = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _repository = new MongoRepository<Usuario>(DbContext.Usuarios);
            IValidator<Usuario> validator = new UsuarioValidator();

            // Mock configuration for JWT
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["Jwt:Key"]).Returns("test_jwt_key_which_is_long_enough_1234567890");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("test_issuer");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("test_audience");

            _service = new UsuarioService(_repository, validator, _configMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_Persists_Usuario_To_Database()
        {
            // Arrange
            var username = $"testuser_{ObjectId.GenerateNewId()}";
            var password = "Password123!";

            // Act
            var registered = await _service.RegisterAsync(username, password);

            // Assert
            Assert.NotNull(registered);
            Assert.NotNull(registered.Id);
            Assert.Equal(username.ToLowerInvariant(), registered.Username);
            Assert.NotEqual(password, registered.PasswordHash); // Debe estar hasheado
            Assert.False(registered.IsActive);
            Assert.Equal("usuario", registered.Role);

            // Verificar que se persistió en DB
            var retrieved = await _repository.FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant());
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task RegisterAsync_Throws_When_Usuario_Already_Exists()
        {
            // Arrange
            var username = $"duplicate_{ObjectId.GenerateNewId()}";
            var password = "Password123!";

            await _service.RegisterAsync(username, password);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.RegisterAsync(username, password)
            );
            Assert.Contains("ya existe", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_Returns_Token_For_Valid_Credentials()
        {
            // Arrange
            var username = $"loginuser_{ObjectId.GenerateNewId()}";
            var password = "SecurePass123!";

            var user = await _service.RegisterAsync(username, password);
            // Activar usuario
            user.IsActive = true;
            await _repository.UpdateAsync(user);

            // Act
            var token = await _service.LoginAsync(username, password);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Contains(".", token); // JWT format: header.payload.signature
        }

        [Fact]
        public async Task LoginAsync_Throws_When_Usuario_Not_Active()
        {
            // Arrange
            var username = $"inactive_{ObjectId.GenerateNewId()}";
            var password = "Pass123!";

            await _service.RegisterAsync(username, password);
            // Dejar IsActive = false (por defecto)

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync(username, password)
            );
            Assert.Contains("activado", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_Throws_When_Credentials_Invalid()
        {
            // Arrange
            var username = $"user_{ObjectId.GenerateNewId()}";
            var password = "Pass123!";

            var user = await _service.RegisterAsync(username, password);
            user.IsActive = true;
            await _repository.UpdateAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.LoginAsync(username, "WrongPassword123!")
            );
            Assert.Contains("inválidas", exception.Message);
        }

        [Fact]
        public async Task IsActiveAsync_Returns_Correct_Status()
        {
            // Arrange
            var username = $"activecheck_{ObjectId.GenerateNewId()}";
            var password = "Pass123!";

            var user = await _service.RegisterAsync(username, password);
            Assert.False(user.IsActive);

            user.IsActive = true;
            await _repository.UpdateAsync(user);

            // Act
            var isActive = await _service.IsActiveAsync(username);

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public async Task RegisterAsync_Normalizes_Username()
        {
            // Arrange & Act
            var user = await _service.RegisterAsync($"  TestUser_{ObjectId.GenerateNewId()}  ", "Pass123!");

            // Assert
            Assert.Equal(user.Username, user.Username.ToLowerInvariant());
        }

        [Fact]
        public async Task LoginAsync_Normalizes_Username()
        {
            // Arrange
            var username = $"logintest_{ObjectId.GenerateNewId()}";
            var password = "Pass123!";

            var user = await _service.RegisterAsync(username, password);
            user.IsActive = true;
            await _repository.UpdateAsync(user);

            // Act
            var token = await _service.LoginAsync($"  {username}  ", password);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(token));
        }
    }
}
