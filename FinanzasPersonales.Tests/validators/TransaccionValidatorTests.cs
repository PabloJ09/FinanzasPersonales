using FluentValidation.TestHelper;
using FinanzasPersonales.Models;
using FinanzasPersonales.Validators;
using System;
using Xunit;

namespace FinanzasPersonales.Tests.Validators
{
    public class TransaccionValidatorTests
    {
        private readonly TransaccionValidator _validator = new TransaccionValidator();

        [Fact]
        public void Should_Have_Error_When_Tipo_Invalid()
        {
            var model = new Transaccion { Tipo = "Bad", Monto = 10M, CategoriaId = "c1", UsuarioId = "u1", Fecha = DateTime.UtcNow };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(t => t.Tipo);
        }

        [Fact]
        public void Should_Have_Error_When_Monto_Not_Positive()
        {
            var model = new Transaccion { Tipo = "Gasto", Monto = 0M, CategoriaId = "c1", UsuarioId = "u1", Fecha = DateTime.UtcNow };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(t => t.Monto);
        }

        [Fact]
        public void Should_Have_Error_When_Fecha_In_Future()
        {
            var model = new Transaccion { Tipo = "Ingreso", Monto = 10M, CategoriaId = "c1", UsuarioId = "u1", Fecha = DateTime.UtcNow.AddDays(1) };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(t => t.Fecha);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Model()
        {
            var model = new Transaccion { Tipo = "Ingreso", Monto = 10M, CategoriaId = "c1", UsuarioId = "u1", Fecha = DateTime.UtcNow };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
