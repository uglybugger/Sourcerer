using System;
using System.Data.Entity;

namespace Sourcerer.FactStore.SqlServer
{
    internal interface IFactStoreDataContext : IDisposable
    {
        DbSet<StreamType> StreamType { get; }
        DbSet<SerializedFact> Facts { get; }

        int SaveChanges();
    }
}