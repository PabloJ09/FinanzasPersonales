using FinanzasPersonales.Database;
using MongoDB.Driver;
using Xunit;

namespace FinanzasPersonales.Tests.Integration
{
    /// <summary>
    /// Clase base para pruebas de integración que proporciona contexto MongoDB real.
    /// </summary>
    public class IntegrationTestBase : IAsyncLifetime
    {
        protected MongoDbFixture MongoFixture { get; }
        protected IMongoDBContext DbContext { get; private set; } = null!;

        public IntegrationTestBase()
        {
            MongoFixture = new MongoDbFixture();
        }

        public virtual async Task InitializeAsync()
        {
            await MongoFixture.InitializeAsync();
            // Crear un contexto de prueba que use la base de datos de prueba
            DbContext = new TestMongoDBContext(MongoFixture.Database);
        }

        public virtual async Task DisposeAsync()
        {
            await MongoFixture.ClearDatabaseAsync();
            await MongoFixture.DisposeAsync();
        }

        /// <summary>
        /// Limpia la base de datos entre tests.
        /// </summary>
        protected async Task ResetDatabaseAsync()
        {
            await MongoFixture.ClearDatabaseAsync();
        }
    }

    /// <summary>
    /// Implementación de IMongoDBContext para pruebas de integración.
    /// Expone las colecciones de la base de datos de prueba.
    /// </summary>
    public class TestMongoDBContext : IMongoDBContext
    {
        private readonly IMongoDatabase _database;

        public TestMongoDBContext(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public string DatabaseName => _database.DatabaseNamespace.DatabaseName;
        public IMongoCollection<FinanzasPersonales.Models.Transaccion> Transacciones =>
            _database.GetCollection<FinanzasPersonales.Models.Transaccion>("Transacciones");
        public IMongoCollection<FinanzasPersonales.Models.Categoria> Categorias =>
            _database.GetCollection<FinanzasPersonales.Models.Categoria>("Categorias");
        public IMongoCollection<FinanzasPersonales.Models.Usuario> Usuarios =>
            _database.GetCollection<FinanzasPersonales.Models.Usuario>("Usuarios");
    }
}
