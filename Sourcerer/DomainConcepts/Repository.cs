using System;
using System.Collections.Concurrent;
using System.Linq;
using Sourcerer.DomainConcepts.Entities;
using Sourcerer.DomainConcepts.Queries;
using ThirdDrawer.Extensions.CollectionExtensionMethods;

namespace Sourcerer.DomainConcepts
{
    public class Repository<T> : IRepository<T> where T : class, IAggregateRoot
    {
        private readonly IQueryModel<T> _queryModel;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ConcurrentDictionary<Guid, T> _newItems = new ConcurrentDictionary<Guid, T>();
        private readonly ConcurrentDictionary<Guid, T> _modifiedItems = new ConcurrentDictionary<Guid, T>();
        private readonly ConcurrentDictionary<Guid, T> _removedItems = new ConcurrentDictionary<Guid, T>();

        public Repository(IQueryModel<T> queryModel, IUnitOfWork unitOfWork)
        {
            _queryModel = queryModel;
            _unitOfWork = unitOfWork;
            _unitOfWork.Completed += OnUnitOfWorkCompleted;
            _unitOfWork.Abandoned += OnUnitOfWorkAbandoned;
        }

        public T GetById(Guid id)
        {
            var item = _modifiedItems.GetOrAdd(id, itemId => _queryModel.GetById(itemId).Clone<T>());
            _unitOfWork.Enlist(item);
            return item;
        }

        public void Add(T item)
        {
            if (item.Id == Guid.Empty) throw new InvalidOperationException("Aggregate roots must have IDs assigned before being added to a repository.");

            _unitOfWork.Enlist(item);
            _newItems[item.Id] = item;
        }

        public void Remove(T item)
        {
            _unitOfWork.Enlist(item);
            _removedItems[item.Id] = item;
        }

        public T[] Query(IQuery<T> query)
        {
            var fromUoW = query.Execute(_modifiedItems.Values.AsQueryable())
                               .Except(_removedItems.Values);

            var fromSnapshot = query.Execute(_queryModel.Items)
                                    .Except(fromUoW)
                                    .Except(_removedItems.Values)
                                    .AsParallel()
                                    .Select(item => item.Clone<T>())
                ;

            fromSnapshot.Do(item => _unitOfWork.Enlist(item)).Done();

            // we re-execute in case there was an orderby clause
            var results = query.Execute(fromUoW.Concat(fromSnapshot)).ToArray();
            return results;
        }

        public TProjection Query<TProjection>(IQuery<T, TProjection> query)
        {
            return query.Execute(_queryModel.Items);
        }

        private void OnUnitOfWorkCompleted(object sender, EventArgs e)
        {
            _queryModel.UpdateAtomically(_newItems.Values, _modifiedItems.Values, _removedItems.Values);
        }

        private void OnUnitOfWorkAbandoned(object sender, EventArgs e)
        {
        }
    }
}