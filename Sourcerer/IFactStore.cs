using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sourcerer.UnitTests
{
    public interface IFactStore
    {
        void AppendAtomically(IFact[] facts);

        IEnumerable<IFact> GetStream<T>(Guid id) where T : IAggregateRoot;
        IEnumerable<Guid> GetAllStreamIds<T>() where T : IAggregateRoot;
    }
}
