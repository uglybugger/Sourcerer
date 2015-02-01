using System;

namespace Sourcerer.DomainConcepts.Facts
{
    public interface IFact
    {
        Guid AggregateRootId { get; }
        string StreamName { get; }
        string EntityTypeName { get; }

        UnitOfWorkProperties UnitOfWorkProperties { get; }
        void SetUnitOfWorkProperties(UnitOfWorkProperties properties);
    }
}