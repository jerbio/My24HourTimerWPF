namespace TilerTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TravelCache : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TravelCaches",
                c => new
                    {
                        TilerUserId = c.String(nullable: false, maxLength: 128),
                        TilerUser_DB_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.TilerUserId)
                .ForeignKey("dbo.AspNetUsers", t => t.TilerUser_DB_Id)
                .Index(t => t.TilerUser_DB_Id);
            
            CreateTable(
                "dbo.LocationCacheEntries",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        LastLookup = c.DateTimeOffset(nullable: false, precision: 7),
                        LastUpdate = c.DateTimeOffset(nullable: false, precision: 7),
                        TimeSpanInMs = c.Double(nullable: false),
                        Distance = c.Double(nullable: false),
                        Medium = c.Int(nullable: false),
                        Medium_DB = c.String(),
                        TaiyeId = c.String(maxLength: 128),
                        KehindeId = c.String(maxLength: 128),
                        TravelCache_TilerUserId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Locations", t => t.KehindeId)
                .ForeignKey("dbo.Locations", t => t.TaiyeId)
                .ForeignKey("dbo.TravelCaches", t => t.TravelCache_TilerUserId)
                .Index(t => t.TaiyeId)
                .Index(t => t.KehindeId)
                .Index(t => t.TravelCache_TilerUserId);
            
            AddColumn("dbo.AspNetUsers", "_TravelCache_TilerUserId", c => c.String(maxLength: 128));
            AddColumn("dbo.AspNetUsers", "TravelCache_TilerUserId", c => c.String(maxLength: 128));
            CreateIndex("dbo.AspNetUsers", "_TravelCache_TilerUserId");
            CreateIndex("dbo.AspNetUsers", "TravelCache_TilerUserId");
            AddForeignKey("dbo.AspNetUsers", "_TravelCache_TilerUserId", "dbo.TravelCaches", "TilerUserId");
            AddForeignKey("dbo.AspNetUsers", "TravelCache_TilerUserId", "dbo.TravelCaches", "TilerUserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "TravelCache_TilerUserId", "dbo.TravelCaches");
            DropForeignKey("dbo.AspNetUsers", "_TravelCache_TilerUserId", "dbo.TravelCaches");
            DropForeignKey("dbo.TravelCaches", "TilerUser_DB_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.LocationCacheEntries", "TravelCache_TilerUserId", "dbo.TravelCaches");
            DropForeignKey("dbo.LocationCacheEntries", "TaiyeId", "dbo.Locations");
            DropForeignKey("dbo.LocationCacheEntries", "KehindeId", "dbo.Locations");
            DropIndex("dbo.LocationCacheEntries", new[] { "TravelCache_TilerUserId" });
            DropIndex("dbo.LocationCacheEntries", new[] { "KehindeId" });
            DropIndex("dbo.LocationCacheEntries", new[] { "TaiyeId" });
            DropIndex("dbo.TravelCaches", new[] { "TilerUser_DB_Id" });
            DropIndex("dbo.AspNetUsers", new[] { "TravelCache_TilerUserId" });
            DropIndex("dbo.AspNetUsers", new[] { "_TravelCache_TilerUserId" });
            DropColumn("dbo.AspNetUsers", "TravelCache_TilerUserId");
            DropColumn("dbo.AspNetUsers", "_TravelCache_TilerUserId");
            DropTable("dbo.LocationCacheEntries");
            DropTable("dbo.TravelCaches");
        }
    }
}
