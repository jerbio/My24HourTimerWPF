namespace TilerTests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class timelineHistoryRename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UpdateHistories", newName: "TimeLineHistories");
            RenameColumn(table: "dbo.EventTimeLines", name: "UpdateHistory_Id", newName: "TimeLineHistory_Id");
            RenameColumn(table: "dbo.TilerEvents", name: "UpdateHistoryId", newName: "TimeLineHistoryId");
            RenameIndex(table: "dbo.EventTimeLines", name: "IX_UpdateHistory_Id", newName: "IX_TimeLineHistory_Id");
            RenameIndex(table: "dbo.TilerEvents", name: "IX_UpdateHistoryId", newName: "IX_TimeLineHistoryId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.TilerEvents", name: "IX_TimeLineHistoryId", newName: "IX_UpdateHistoryId");
            RenameIndex(table: "dbo.EventTimeLines", name: "IX_TimeLineHistory_Id", newName: "IX_UpdateHistory_Id");
            RenameColumn(table: "dbo.TilerEvents", name: "TimeLineHistoryId", newName: "UpdateHistoryId");
            RenameColumn(table: "dbo.EventTimeLines", name: "TimeLineHistory_Id", newName: "UpdateHistory_Id");
            RenameTable(name: "dbo.TimeLineHistories", newName: "UpdateHistories");
        }
    }
}
