using System.Data.Entity;

namespace Sourcerer.FactStore.SqlServer
{
    internal class FactStoreDataContext : DbContext, IFactStoreDataContext
    {
        public FactStoreDataContext(string connectionString) : base(connectionString)
        {
        }

        public DbSet<StreamType> StreamType { get; set; }
        public DbSet<SerializedFact> Facts { get; set; }
    }
}