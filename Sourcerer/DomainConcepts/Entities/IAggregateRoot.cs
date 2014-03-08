using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.DomainConcepts.Entities
{
    public interface IAggregateRoot
    {
        Guid Id { get; }
        IEnumerable<IFact> GetAndClearPendingFacts();
        Guid RevisionId { get; set; }
    }
}