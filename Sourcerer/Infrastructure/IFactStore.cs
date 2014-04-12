using System;
using System.Collections.Generic;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure
{
    public interface IFactStore
    {
        void AppendAtomically(IFact[] facts);

        IEnumerable<FactAbout<T>> GetStream<T>(Guid id) where T : IAggregateRoot;
        IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot;

        /// <summary>
        ///     This returns a firehose of all facts ever, grouped by unit of work ID. Use it for migrations and the like.
        /// </summary>
        IEnumerable<IGrouping<Guid, IFact>> GetAllFactsGroupedByUnitOfWork();

        void ImportFrom(IEnumerable<IFact> facts);

        bool HasFacts { get; }
    }
}