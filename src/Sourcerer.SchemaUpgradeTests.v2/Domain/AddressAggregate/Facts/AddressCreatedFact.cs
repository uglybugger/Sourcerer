using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts
{
    [Fact("{F945E8D9-94CA-492F-B2EF-A335E9491510}")]
    public class AddressCreatedFact : FactAbout<Address>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}