using System;
using Sourcerer.DomainConcepts.Entities;

namespace Sourcerer.DomainConcepts
{
    public interface IUnitOfWork : IDisposable
    {
        void Enlist(IAggregateRoot item);

        EventHandler<EventArgs> Completed { get; set; }
        void Complete();

        EventHandler<EventArgs> Abandoned { get; set; }
        void Abandon();
    }
}