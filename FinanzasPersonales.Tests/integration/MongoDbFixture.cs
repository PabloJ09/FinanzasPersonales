using MongoDB.Driver;
using Xunit;

namespace FinanzasPersonales.Tests.Integration
{
    /// <summary>
    /// Fixture que maneja conexión a MongoDB local para pruebas de integración.
    /// Requiere: MongoDB ejecutándose en `localhost:27017` (levantar la instancia localmente antes de ejecutar tests)
    /// </summary>
    public class MongoDbFixture : IAsyncLifetime
    {
        private IMongoClient? _client;
        private IMongoDatabase? _database;
        private const string ConnectionString = "mongodb://localhost:27017";
        private const string DatabaseName = "FinanzasTest";

        public IMongoClient Client => _client ?? throw new InvalidOperationException("Cliente no inicializado");
        public IMongoDatabase Database => _database ?? throw new InvalidOperationException("Base de datos no inicializada");

        public async Task InitializeAsync()
        {
            // Crear cliente e intentar conectar a MongoDB local
            _client = new MongoClient(ConnectionString);
            _database = _client.GetDatabase(DatabaseName);

            try
            {
                // Verificar conectividad
                await _client.GetDatabase("admin").RunCommandAsync<dynamic>(
                    new MongoDB.Bson.BsonDocument("ping", 1)
                );
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "No se puede conectar a MongoDB en localhost:27017. " +
                    "Por favor, inicia MongoDB localmente antes de ejecutar las pruebas.",
                    ex
                );
            }

            // Limpiar base de datos de pruebas previas
            await ClearDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            if (_database != null)
            {
                try
                {
                    await ClearDatabaseAsync();
                }
                catch
                {
                    // Ignorar errores al limpiar en destrucción
                }
            }
        }

        /// <summary>
        /// Limpia todas las colecciones de la base de datos para tests aislados.
        /// </summary>
        public async Task ClearDatabaseAsync()
        {
            if (_database != null)
            {
                try
                {
                    var collections = await _database.ListCollectionNamesAsync();
                    await collections.ForEachAsync(async collectionName =>
                    {
                        try
                        {
                            await _database.DropCollectionAsync(collectionName);
                        }
                        catch
                        {
                            // Ignorar si la colección no existe
                        }
                    });
                }
                catch
                {
                    // Ignorar si la base de datos no existe
                }
            }
        }

        /// <summary>
        /// Obtiene una colección tipada de la base de datos.
        /// </summary>
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }
    }
}
