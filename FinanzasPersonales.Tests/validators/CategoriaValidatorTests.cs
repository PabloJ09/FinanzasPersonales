using FluentValidation.TestHelper;
using FinanzasPersonales.Models;
using FinanzasPersonales.Validators;
using Xunit;

namespace FinanzasPersonales.Tests.Validators
{
    public class CategoriaValidatorTests
    {
        private readonly CategoriaValidator _validator = new CategoriaValidator();

        [Fact]
        public void Should_Have_Error_When_Nombre_Is_Empty()
        {
            var model = new Categoria { Nombre = string.Empty, Tipo = "Gasto", UsuarioId = "u1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Fact]
        public void Should_Have_Error_When_Tipo_Is_Invalid()
        {
            var model = new Categoria { Nombre = "X", Tipo = "Bad", UsuarioId = "u1" };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(c => c.Tipo);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new Categoria { Nombre = "Comida", Tipo = "Gasto", UsuarioId = "u1" };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
