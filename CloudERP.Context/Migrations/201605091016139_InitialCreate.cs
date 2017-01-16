namespace CloudERP.Context.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HR_Attendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TimeIn = c.DateTime(nullable: false),
                        TimeOut = c.DateTime(),
                        HR_Schedule_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_Schedule", t => t.HR_Schedule_Id)
                .Index(t => t.HR_Schedule_Id);
            
            CreateTable(
                "dbo.HR_Schedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        From = c.DateTime(nullable: false),
                        To = c.DateTime(),
                        HR_MainSchedule_Id = c.Int(),
                        HR_Users_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_MainSchedule", t => t.HR_MainSchedule_Id)
                .ForeignKey("dbo.HR_Users", t => t.HR_Users_Id, cascadeDelete: true)
                .Index(t => t.HR_MainSchedule_Id)
                .Index(t => t.HR_Users_Id);
            
            CreateTable(
                "dbo.HR_MainSchedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ScheduleNickname = c.String(),
                        TimeIn = c.DateTime(),
                        TimeOut = c.DateTime(),
                        IsDefault = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HR_Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        FirstName = c.String(),
                        MiddleName = c.String(),
                        LastName = c.String(),
                        NfcRfidSerial = c.String(),
                        Image = c.Binary(),
                        IsDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HR_Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HR_UserRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        From = c.DateTime(nullable: false),
                        To = c.DateTime(),
                        HR_Roles_Id = c.Int(),
                        HR_Users_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_Roles", t => t.HR_Roles_Id)
                .ForeignKey("dbo.HR_Users", t => t.HR_Users_Id, cascadeDelete: true)
                .Index(t => t.HR_Roles_Id)
                .Index(t => t.HR_Users_Id);
            
            CreateTable(
                "dbo.SITE_PendingTask",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        PendingTask = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HR_Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SITE_PendingTask", "UserId", "dbo.HR_Users");
            DropForeignKey("dbo.HR_UserRoles", "HR_Users_Id", "dbo.HR_Users");
            DropForeignKey("dbo.HR_UserRoles", "HR_Roles_Id", "dbo.HR_Roles");
            DropForeignKey("dbo.HR_Attendance", "HR_Schedule_Id", "dbo.HR_Schedule");
            DropForeignKey("dbo.HR_Schedule", "HR_Users_Id", "dbo.HR_Users");
            DropForeignKey("dbo.HR_Schedule", "HR_MainSchedule_Id", "dbo.HR_MainSchedule");
            DropIndex("dbo.SITE_PendingTask", new[] { "UserId" });
            DropIndex("dbo.HR_UserRoles", new[] { "HR_Users_Id" });
            DropIndex("dbo.HR_UserRoles", new[] { "HR_Roles_Id" });
            DropIndex("dbo.HR_Schedule", new[] { "HR_Users_Id" });
            DropIndex("dbo.HR_Schedule", new[] { "HR_MainSchedule_Id" });
            DropIndex("dbo.HR_Attendance", new[] { "HR_Schedule_Id" });
            DropTable("dbo.SITE_PendingTask");
            DropTable("dbo.HR_UserRoles");
            DropTable("dbo.HR_Roles");
            DropTable("dbo.HR_Users");
            DropTable("dbo.HR_MainSchedule");
            DropTable("dbo.HR_Schedule");
            DropTable("dbo.HR_Attendance");
        }
    }
}
