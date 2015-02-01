using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace TicketSeller.App.Domain.CustomerAggregate.Facts
{
    [Fact("{5910FFD0-4571-4B98-BD20-3740647F7640}")]
    public class CustomerCreatedFact : FactAbout<Customer>
    {
        public string Name { get; set; }
    }
}