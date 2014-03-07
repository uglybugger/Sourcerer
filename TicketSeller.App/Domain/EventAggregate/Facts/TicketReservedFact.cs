using System;
using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace TicketSeller.App.Domain.EventAggregate.Facts
{
    [Fact("{2BE5115B-4CA3-4403-8FAA-44969B88A12C}")]
    public class TicketReservedFact : FactAbout<Event>
    {
        public Guid CustomerId { get; set; }
        public int NumTickets { get; set; }
    }
}