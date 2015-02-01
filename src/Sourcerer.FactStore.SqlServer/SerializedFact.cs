using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sourcerer.FactStore.SqlServer
{
    internal class SerializedFact
    {
        [Key]
        [Column(Order = 0)]
        public Guid UnitOfWorkId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SequenceNumber { get; set; }

        public Guid StreamTypeId { get; set; }
        public Guid AggregateRootId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public byte[] FactBlob { get; set; }
    }
}