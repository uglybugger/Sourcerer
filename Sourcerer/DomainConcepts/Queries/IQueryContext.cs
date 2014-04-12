using System;
using System.Linq;

namespace Sourcerer
{
    public interface IQueryContext<TAggregateRoot> : IDisposable
    {
        TProjection Query<TProjection>(Func<IQueryable<TAggregateRoot>, TProjection> query);
    }
}