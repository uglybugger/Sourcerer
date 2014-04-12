using System;
using System.Collections.Generic;
using Sourcerer.DomainConcepts.Entities;
using TicketSeller.App.Domain.CustomerAggregate;
using TicketSeller.App.Domain.EventAggregate.Facts;

namespace TicketSeller.App.Domain.EventAggregate
{
    [Serializable]
    public class Event : AggregateRoot
    {
        private readonly List<TicketSale> _ticketSales = new List<TicketSale>();
        private int _ticketsSold;

        private Event()
        {
        }

        public static Event Create(string name, int capacity)
        {
            var fact = new EventCreatedFact
                       {
                           AggregateRootId = Guid.NewGuid(),
                           Name = name,
                           Capacity = capacity,
                       };
            var @event = new Event();
            @event.Append(fact);
            @event.Apply(fact);
            return @event;
        }

        public void Apply(EventCreatedFact fact)
        {
            Id = fact.AggregateRootId;
            Name = fact.Name;
            Capacity = fact.Capacity;
        }

        public string Name { get; private set; }
        public int Capacity { get; private set; }

        internal bool TryReserveTicketsFor(Customer customer, int numTickets)
        {
            var requiredCapacity = _ticketsSold + numTickets;
            if (Capacity < requiredCapacity) return false;

            var fact = new TicketReservedFact
                       {
                           AggregateRootId = Id,
                           CustomerId = customer.Id,
                           NumTickets = numTickets,
                       };

            Append(fact);
            Apply(fact);
            return true;
        }

        public void Apply(TicketReservedFact fact)
        {
            var ticketSale = new TicketSale(fact.CustomerId, fact.NumTickets);
            _ticketSales.Add(ticketSale);
            _ticketsSold += fact.NumTickets;
        }

        public int GetRemainingTicketCount()
        {
            return Capacity - _ticketsSold;
        }
    }
}