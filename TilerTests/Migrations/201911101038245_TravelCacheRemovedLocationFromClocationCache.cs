namespace TilerTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TravelCacheRemovedLocationFromClocationCache : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationCacheEntries", "KehindeId", "dbo.Locations");
            DropForeignKey("dbo.LocationCacheEntries", "TaiyeId", "dbo.Locations");
            DropIndex("dbo.LocationCacheEntries", new[] { "TaiyeId" });
            DropIndex("dbo.LocationCacheEntries", new[] { "KehindeId" });
            AlterColumn("dbo.LocationCacheEntries", "TaiyeId", c => c.String());
            AlterColumn("dbo.LocationCacheEntries", "KehindeId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LocationCacheEntries", "KehindeId", c => c.String(maxLength: 128));
            AlterColumn("dbo.LocationCacheEntries", "TaiyeId", c => c.String(maxLength: 128));
            CreateIndex("dbo.LocationCacheEntries", "KehindeId");
            CreateIndex("dbo.LocationCacheEntries", "TaiyeId");
            AddForeignKey("dbo.LocationCacheEntries", "TaiyeId", "dbo.Locations", "Id");
            AddForeignKey("dbo.LocationCacheEntries", "KehindeId", "dbo.Locations", "Id");
        }
    }
}
