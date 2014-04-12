namespace Sourcerer.FactStore.SqlServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HelloWorld : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SerializedFacts",
                c => new
                    {
                        UnitOfWorkId = c.Guid(nullable: false),
                        SequenceNumber = c.Int(nullable: false),
                        StreamTypeId = c.Guid(nullable: false),
                        AggregateRootId = c.Guid(nullable: false),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        FactBlob = c.Binary(),
                    })
                .PrimaryKey(t => new { t.UnitOfWorkId, t.SequenceNumber });
            
            CreateTable(
                "dbo.StreamTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        StreamName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.StreamTypes");
            DropTable("dbo.SerializedFacts");
        }
    }
}
