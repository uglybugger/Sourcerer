using System;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;

namespace Sourcerer.DomainConcepts
{
    public class Repository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryableSnapshot _snapshot;

        public Repository(IUnitOfWork unitOfWork, IQueryableSnapshot snapshot)
        {
            _unitOfWork = unitOfWork;
            _snapshot = snapshot;
        }

        public T GetById(Guid id)
        {
            var clone = TryGetById(id);
            if (clone == null) throw new InvalidOperationException("Aggregate root does not exist");
            return clone;
        }

        public T TryGetById(Guid id)
        {
            var aggregateRoot = _snapshot.TryGetById<T>(id);
            if (aggregateRoot == null) return null;

            var clone = aggregateRoot.BinaryClone();

            _unitOfWork.EnlistInTransaction(clone);
            return clone;
        }

        public void Add(T item)
        {
            if (item.Id == Guid.Empty) throw new InvalidOperationException("Aggregate roots must have IDs assigned before being added to a repository.");
            _unitOfWork.EnlistInTransaction(item);
        }

        public void Remove(T item)
        {
            _unitOfWork.EnlistInTransaction(item);
        }

        public T[] Query(IQuery<T> query)
        {
            //FIXME this isn't strictly correct. We're still querying against the snapshotted versions rather than the combined enlisted + remaining snapshotted ones
            return query.Execute(_snapshot.Items<T>())
                        .Select(storedEntity => FindEnlistedEntityOrCloneSnapshottedEntity(storedEntity))
                        .ToArray();
        }

        public TProjection Query<TProjection>(IQuery<T, TProjection> query)
        {
            return query.Execute(_snapshot.Items<T>());
        }

        private T FindEnlistedEntityOrCloneSnapshottedEntity(T storedEntity)
        {
            // is this entity already participating in our current transaction? return that if so
            var entityInTransaction = _unitOfWork.TryGetEnlistedAggregateRoot<T>(storedEntity.Id);
            if (entityInTransaction != null) return entityInTransaction;

            // otherwise do a binary clone and enlist the clone
            var clone = storedEntity.BinaryClone();
            _unitOfWork.EnlistInTransaction(clone);
            return clone;
        }
    }
}