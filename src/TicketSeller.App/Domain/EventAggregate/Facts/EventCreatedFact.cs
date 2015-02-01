using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace TicketSeller.App.Domain.EventAggregate.Facts
{
    [Fact("{74FFE4EB-2356-4C4F-93DE-9D17ED70B19D}")]
    public class EventCreatedFact : FactAbout<Event>
    {
        public string Name { get; set; }
        public int Capacity { get; set; }
    }
}