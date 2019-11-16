namespace TilerTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locationValidation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LocationCacheEntries", "TravelCache_TilerUserId", "dbo.TravelCaches");
            DropForeignKey("dbo.AspNetUsers", "_TravelCache_TilerUserId", "dbo.TravelCaches");
            DropForeignKey("dbo.AspNetUsers", "TravelCache_TilerUserId", "dbo.TravelCaches");
            RenameColumn(table: "dbo.AspNetUsers", name: "_TravelCache_TilerUserId", newName: "_TravelCache_Id");
            RenameColumn(table: "dbo.AspNetUsers", name: "TravelCache_TilerUserId", newName: "TravelCache_Id");
            RenameColumn(table: "dbo.LocationCacheEntries", name: "TravelCache_TilerUserId", newName: "TravelCache_Id");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX__TravelCache_TilerUserId", newName: "IX__TravelCache_Id");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_TravelCache_TilerUserId", newName: "IX_TravelCache_Id");
            RenameIndex(table: "dbo.LocationCacheEntries", name: "IX_TravelCache_TilerUserId", newName: "IX_TravelCache_Id");
            DropPrimaryKey("dbo.TravelCaches");
            AddColumn("dbo.TravelCaches", "Id", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.TravelCaches", "Id");
            AddForeignKey("dbo.LocationCacheEntries", "TravelCache_Id", "dbo.TravelCaches", "Id");
            AddForeignKey("dbo.AspNetUsers", "_TravelCache_Id", "dbo.TravelCaches", "Id");
            AddForeignKey("dbo.AspNetUsers", "TravelCache_Id", "dbo.TravelCaches", "Id");
            DropColumn("dbo.TravelCaches", "TilerUserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TravelCaches", "TilerUserId", c => c.String(nullable: false, maxLength: 128));
            DropForeignKey("dbo.AspNetUsers", "TravelCache_Id", "dbo.TravelCaches");
            DropForeignKey("dbo.AspNetUsers", "_TravelCache_Id", "dbo.TravelCaches");
            DropForeignKey("dbo.LocationCacheEntries", "TravelCache_Id", "dbo.TravelCaches");
            DropPrimaryKey("dbo.TravelCaches");
            DropColumn("dbo.TravelCaches", "Id");
            AddPrimaryKey("dbo.TravelCaches", "TilerUserId");
            RenameIndex(table: "dbo.LocationCacheEntries", name: "IX_TravelCache_Id", newName: "IX_TravelCache_TilerUserId");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_TravelCache_Id", newName: "IX_TravelCache_TilerUserId");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX__TravelCache_Id", newName: "IX__TravelCache_TilerUserId");
            RenameColumn(table: "dbo.LocationCacheEntries", name: "TravelCache_Id", newName: "TravelCache_TilerUserId");
            RenameColumn(table: "dbo.AspNetUsers", name: "TravelCache_Id", newName: "TravelCache_TilerUserId");
            RenameColumn(table: "dbo.AspNetUsers", name: "_TravelCache_Id", newName: "_TravelCache_TilerUserId");
            AddForeignKey("dbo.AspNetUsers", "TravelCache_TilerUserId", "dbo.TravelCaches", "TilerUserId");
            AddForeignKey("dbo.AspNetUsers", "_TravelCache_TilerUserId", "dbo.TravelCaches", "TilerUserId");
            AddForeignKey("dbo.LocationCacheEntries", "TravelCache_TilerUserId", "dbo.TravelCaches", "TilerUserId");
        }
    }
}
