// Database/MongoIndexSetup.cs
using FinanzasPersonales.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Diagnostics.CodeAnalysis;

namespace FinanzasPersonales.Database
{
    [ExcludeFromCodeCoverage]
    public static class MongoIndexSetup
    {
        
        public static async Task CreateIndexesAsync(IMongoDBContext context, ILogger? logger = null)
        {
            // --- Usuarios (usamos la colección expuesta por IMongoDBContext)
            var usuarios = context.Usuarios;

            var usernameIndex = Builders<Usuario>.IndexKeys.Ascending(u => u.Username);
            var usernameOptions = new CreateIndexOptions { Unique = true, Name = "idx_usuario_username_unique" };
            await usuarios.Indexes.CreateOneAsync(new CreateIndexModel<Usuario>(usernameIndex, usernameOptions));
            logger?.LogInformation("Índice Usuarios.Username (unique) creado/verificado.");

            var roleIndex = Builders<Usuario>.IndexKeys.Ascending(u => u.Role);
            await usuarios.Indexes.CreateOneAsync(new CreateIndexModel<Usuario>(roleIndex, new CreateIndexOptions { Name = "idx_usuario_role" }));
            logger?.LogInformation("Índice Usuarios.Role creado/verificado.");

            // --- Categorias
            var categorias = context.Categorias;

            var catUsuarioIndex = Builders<Categoria>.IndexKeys.Ascending(c => c.UsuarioId);
            await categorias.Indexes.CreateOneAsync(new CreateIndexModel<Categoria>(catUsuarioIndex, new CreateIndexOptions { Name = "idx_categoria_usuario" }));
            logger?.LogInformation("Índice Categorias.UsuarioId creado/verificado.");

            var catCompuesto = Builders<Categoria>.IndexKeys
                .Ascending(c => c.UsuarioId)
                .Ascending(c => c.Nombre);
            var catCompuestoOptions = new CreateIndexOptions { Unique = true, Name = "idx_categoria_usuario_nombre_unique" };
            await categorias.Indexes.CreateOneAsync(new CreateIndexModel<Categoria>(catCompuesto, catCompuestoOptions));
            logger?.LogInformation("Índice Categorias.UsuarioId+Nombre (unique) creado/verificado.");

            // --- Transacciones
            var trans = context.Transacciones;

            var tUsuarioIndex = Builders<Transaccion>.IndexKeys.Ascending(t => t.UsuarioId);
            var tCategoriaIndex = Builders<Transaccion>.IndexKeys.Ascending(t => t.CategoriaId);
            var tFechaDesc = Builders<Transaccion>.IndexKeys.Descending(t => t.Fecha);
            var tUsuarioFecha = Builders<Transaccion>.IndexKeys
                .Ascending(t => t.UsuarioId)
                .Descending(t => t.Fecha);

            var transIndexes = new List<CreateIndexModel<Transaccion>>
            {
                new CreateIndexModel<Transaccion>(tUsuarioIndex, new CreateIndexOptions { Name = "idx_trans_usuario" }),
                new CreateIndexModel<Transaccion>(tCategoriaIndex, new CreateIndexOptions { Name = "idx_trans_categoria" }),
                new CreateIndexModel<Transaccion>(tFechaDesc, new CreateIndexOptions { Name = "idx_trans_fecha_desc" }),
                new CreateIndexModel<Transaccion>(tUsuarioFecha, new CreateIndexOptions { Name = "idx_trans_usuario_fecha" })
            };

            await trans.Indexes.CreateManyAsync(transIndexes);
            logger?.LogInformation("Índices Transacciones creados/verificados.");
        }
    }
}
