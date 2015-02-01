using System.Collections.Generic;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.Infrastructure.Migrations
{
    public class MigrationContext
    {
        private readonly List<IFact> _facts = new List<IFact>();

        public IEnumerable<IFact> Facts
        {
            get { return _facts; }
        }

        public void Append(IFact fact)
        {
            _facts.Add(fact);
        }
    }
}