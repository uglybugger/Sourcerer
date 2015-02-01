using System.Data.Entity.Infrastructure;

namespace Sourcerer.FactStore.SqlServer
{
    internal class FactStoreDataContextFactory : IDbContextFactory<FactStoreDataContext>
    {
        public FactStoreDataContext Create()
        {
            return new FactStoreDataContext(@"Server=.\SQLEXPRESS;Database=SourcererIntegrationTests;Trusted_Connection=True;");
        }
    }
}