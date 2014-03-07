using System;
using Sourcerer.DomainConcepts.Entities;
using TicketSeller.App.Domain.CustomerAggregate.Facts;
using TicketSeller.App.Domain.EventAggregate;

namespace TicketSeller.App.Domain.CustomerAggregate
{
    [Serializable]
    public class Customer : AggregateRoot
    {
        private Customer()
        {
        }

        public static Customer Create(string name)
        {
            var fact = new CustomerCreatedFact
                       {
                           AggregateRootId = Guid.NewGuid(),
                           Name = name,
                       };

            var customer = new Customer();
            customer.Append(fact);
            customer.Apply(fact);
            return customer;
        }

        public void Apply(CustomerCreatedFact fact)
        {
            Id = fact.AggregateRootId;
            Name = fact.Name;
        }

        public string Name { get; private set; }

        public bool TryReserveTicketsFor(Event @event, int numTickets)
        {
            return @event.TryReserveTicketsFor(this, numTickets);
        }
    }
}