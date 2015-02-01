using System;
using System.Collections.Generic;
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
        private readonly HashSet<T> _addedItems = new HashSet<T>();
        private readonly HashSet<T> _modifiedItems = new HashSet<T>();
        private readonly HashSet<T> _removedItems = new HashSet<T>();

        public Repository(IQueryModel<T> queryModel, IUnitOfWork unitOfWork)
        {
            _queryModel = queryModel;
            _unitOfWork = unitOfWork;
            _unitOfWork.Completed += OnUnitOfWorkCompleted;
            _unitOfWork.Abandoned += OnUnitOfWorkAbandoned;
        }

        public T GetById(Guid id)
        {
            var item = _queryModel.GetById(id);

            _unitOfWork.Enlist(item);
            _modifiedItems.Add(item);
            return item;
        }

        public void Add(T item)
        {
            if (item.Id == Guid.Empty) throw new InvalidOperationException("Aggregate roots must have IDs assigned before being added to a repository.");

            _unitOfWork.Enlist(item);
            _addedItems.Add(item);
            _queryModel.Add(item);
        }

        public void Remove(T item)
        {
            _unitOfWork.Enlist(item);
            _removedItems.Add(item);
            _queryModel.Remove(item);
        }

        public T[] Query(Func<IQueryable<T>, T[]> query)
        {
            var results = query(_queryModel.Items);
            results.Do(item => _unitOfWork.Enlist(item))
                   .Done();
            return results;
        }

        private void OnUnitOfWorkCompleted(object sender, EventArgs e)
        {
        }

        private void OnUnitOfWorkAbandoned(object sender, EventArgs e)
        {
            _queryModel.Revert(_addedItems, _modifiedItems, _removedItems);
        }
    }
}