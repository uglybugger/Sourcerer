using System.Linq;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts.Queries
{
    public interface IQuery<TEntity>
    {
        IQueryable<TEntity> Execute(IQueryable<TEntity> source);
    }

    public interface IQuery<TEntity, TProjection> where TEntity : IAggregateRoot
    {
        TProjection Execute(IQueryable<TEntity> source);
    }
}