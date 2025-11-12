using FluentValidation;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.Database.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace FinanzasPersonales.Tests.Services
{
    public class ServiceGuardsTests
    {
        [Fact]
        public void CategoriaService_Constructor_Throws_On_Null()
        {
            var validator = new Mock<IValidator<Categoria>>();
            Assert.Throws<ArgumentNullException>(() => new CategoriaService(null!, validator.Object));
            var repo = new Mock<IRepository<Categoria>>();
            Assert.Throws<ArgumentNullException>(() => new CategoriaService(repo.Object, null!));
        }

        [Fact]
        public void TransaccionService_Constructor_Throws_On_Null()
        {
            var validator = new Mock<IValidator<FinanzasPersonales.Models.Transaccion>>();
            Assert.Throws<ArgumentNullException>(() => new TransaccionService(null!, validator.Object));
            var repo = new Mock<IRepository<FinanzasPersonales.Models.Transaccion>>();
            Assert.Throws<ArgumentNullException>(() => new TransaccionService(repo.Object, null!));
        }

        [Fact]
        public void UsuarioService_Constructor_Throws_On_Null()
        {
            var validator = new Mock<IValidator<Usuario>>();
            var repo = new Mock<IRepository<Usuario>>();
            var config = new Mock<IConfiguration>();

            Assert.Throws<ArgumentNullException>(() => new UsuarioService(null!, validator.Object, config.Object));
            Assert.Throws<ArgumentNullException>(() => new UsuarioService(repo.Object, null!, config.Object));
            Assert.Throws<ArgumentNullException>(() => new UsuarioService(repo.Object, validator.Object, null!));
        }

        [Fact]
        public async System.Threading.Tasks.Task CategoriaService_CreateAsync_Throws_On_Null()
        {
            var repo = new Mock<IRepository<Categoria>>();
            var validator = new Mock<IValidator<Categoria>>();
            var svc = new CategoriaService(repo.Object, validator.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => svc.CreateAsync(null!));
        }

        [Fact]
        public async System.Threading.Tasks.Task TransaccionService_CreateAsync_Throws_On_Null()
        {
            var repo = new Mock<IRepository<FinanzasPersonales.Models.Transaccion>>();
            var validator = new Mock<IValidator<FinanzasPersonales.Models.Transaccion>>();
            var svc = new TransaccionService(repo.Object, validator.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => svc.CreateAsync(null!));
        }

        [Fact]
        public async System.Threading.Tasks.Task UsuarioService_RegisterAsync_Throws_On_EmptyArgs()
        {
            var repo = new Mock<IRepository<Usuario>>();
            var validator = new Mock<IValidator<Usuario>>();
            var config = new Mock<IConfiguration>();
            var svc = new UsuarioService(repo.Object, validator.Object, config.Object);

            await Assert.ThrowsAsync<ArgumentException>(() => svc.RegisterAsync("", "pwd"));
            await Assert.ThrowsAsync<ArgumentException>(() => svc.RegisterAsync("user", ""));
        }
    }
}
