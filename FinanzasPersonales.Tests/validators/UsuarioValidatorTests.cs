using FluentValidation.TestHelper;
using FinanzasPersonales.Models;
using FinanzasPersonales.Validators;
using Xunit;

namespace FinanzasPersonales.Tests.Validators
{
    public class UsuarioValidatorTests
    {
        private readonly UsuarioValidator _validator = new UsuarioValidator();

        [Fact]
        public void Should_Have_Error_When_Username_Invalid()
        {
            var model = new Usuario { Username = "a", PasswordHash = "hash", Role = "usuario" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(u => u.Username);
        }

        [Fact]
        public void Should_Have_Error_When_PasswordHash_Empty()
        {
            var model = new Usuario { Username = "user1", PasswordHash = string.Empty, Role = "usuario" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(u => u.PasswordHash);
        }

        [Fact]
        public void Should_Have_Error_When_Role_Invalid()
        {
            var model = new Usuario { Username = "user1", PasswordHash = "hash", Role = "bad" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(u => u.Role);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_User()
        {
            var model = new Usuario { Username = "user1", PasswordHash = "h", Role = "usuario" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
