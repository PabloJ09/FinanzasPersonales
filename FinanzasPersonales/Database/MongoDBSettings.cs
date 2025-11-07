using System.Diagnostics.CodeAnalysis;
namespace FinanzasPersonales.Database
{
    [ExcludeFromCodeCoverage]
    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
