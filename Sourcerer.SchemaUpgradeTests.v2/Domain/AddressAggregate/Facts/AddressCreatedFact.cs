using Sourcerer.DomainConcepts.Facts;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.AddressAggregate.Facts
{
    public class AddressCreatedFact: FactAbout<Address>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}