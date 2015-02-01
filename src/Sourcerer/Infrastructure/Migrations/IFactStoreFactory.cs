namespace Sourcerer.Infrastructure.Migrations
{
    public interface IFactStoreFactory
    {
        IFactStore GetFactStore(int schemaVersion);
    }
}