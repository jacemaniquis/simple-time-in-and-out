namespace CloudERP.Context.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTimesheet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HR_Timesheet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        Notes = c.String(),
                        HR_Users_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_Users", t => t.HR_Users_Id, cascadeDelete: true)
                .Index(t => t.HR_Users_Id);
            
            CreateTable(
                "dbo.HR_Timesheet_TagMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HR_Timesheet_Id = c.Int(nullable: false),
                        HR_Timesheet_Tags_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_Timesheet", t => t.HR_Timesheet_Id, cascadeDelete: true)
                .ForeignKey("dbo.HR_Timesheet_Tags", t => t.HR_Timesheet_Tags_Id, cascadeDelete: true)
                .Index(t => t.HR_Timesheet_Id)
                .Index(t => t.HR_Timesheet_Tags_Id);
            
            CreateTable(
                "dbo.HR_Timesheet_Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Tag = c.String(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HR_Timesheet_TagMapping", "HR_Timesheet_Tags_Id", "dbo.HR_Timesheet_Tags");
            DropForeignKey("dbo.HR_Timesheet_TagMapping", "HR_Timesheet_Id", "dbo.HR_Timesheet");
            DropForeignKey("dbo.HR_Timesheet", "HR_Users_Id", "dbo.HR_Users");
            DropIndex("dbo.HR_Timesheet_TagMapping", new[] { "HR_Timesheet_Tags_Id" });
            DropIndex("dbo.HR_Timesheet_TagMapping", new[] { "HR_Timesheet_Id" });
            DropIndex("dbo.HR_Timesheet", new[] { "HR_Users_Id" });
            DropTable("dbo.HR_Timesheet_Tags");
            DropTable("dbo.HR_Timesheet_TagMapping");
            DropTable("dbo.HR_Timesheet");
        }
    }
}
