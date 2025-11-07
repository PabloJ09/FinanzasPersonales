using MongoDB.Driver;
using FinanzasPersonales.Models;

using System.Diagnostics.CodeAnalysis;
namespace FinanzasPersonales.Database
{
    public interface IMongoDBContext
    {
        IMongoCollection<Transaccion> Transacciones { get; }
        IMongoCollection<Categoria> Categorias { get; }
        IMongoCollection<FinanzasPersonales.Models.Usuario> Usuarios { get; }
        string DatabaseName { get; }
    }
}
