using Sourcerer.DomainConcepts;
using Sourcerer.DomainConcepts.Facts;
using Sourcerer.SchemaUpgradeTests.v2.Domain.StudentAggregate;

namespace Sourcerer.SchemaUpgradeTests.v2.Domain.SchemaMigrations.v00001
{
    [Fact("{7303E308-E2F6-4AE6-B302-0D90337163EE}")]
    public class StudentChangedAddressFact : FactAbout<Student>
    {
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
    }
}