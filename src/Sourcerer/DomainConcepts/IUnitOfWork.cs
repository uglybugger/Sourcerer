using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts
{
    public interface IUnitOfWork : IDisposable
    {
        void Enlist(IAggregateRoot item);
        IEnumerable<IAggregateRoot> EnlistedItems { get; }

        EventHandler<EventArgs> Completed { get; set; }
        void Complete();

        EventHandler<EventArgs> Abandoned { get; set; }
        void Abandon();
    }
}