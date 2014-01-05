using System;
using System.Collections.Generic;

namespace Sourcerer.UnitTests
{
    public interface IAggregateRoot
    {
        Guid Id { get; }
        IEnumerable<IFact> GetAndClearPendingFacts();
        long RevisionNumber { get; set; }
    }
}