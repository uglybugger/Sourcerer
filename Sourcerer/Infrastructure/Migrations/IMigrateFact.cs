using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure.Migrations
{
    public interface IMigrateFact
    {
        IEnumerable<IFact> Migrate(IFact fact);
    }

    public interface IMigrateFact<TFact> : IMigrateFact
    {
        IEnumerable<IFact> Migrate(TFact fact);
    }
}