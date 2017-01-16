using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudERP.Context.Models;


namespace CloudERP.Context
{
    public class CloudERPDbContext : DbContext
    {
        public CloudERPDbContext()
            : base("CloudERPDbContext")
        {

        }

        public DbSet<HR_Users> HR_Users { get; set; }

        public DbSet<HR_MainSchedule> HR_MainSchedule { get; set; }

        public DbSet<HR_Roles> HR_Roles { get; set; }


        public DbSet<HR_Schedule> HR_Schedule { get; set; }

        public DbSet<HR_UserRoles> HR_UserRoles { get; set; }

        public DbSet<HR_Attendance> HR_Attendance { get; set; }

        public DbSet<HR_Timesheet> HR_Timesheet { get; set; }



        public DbSet<SITE_PendingTask> SITE_PendingTask { get; set; }



        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Entity<HR_Schedule>()
                .HasRequired(p => p.HR_Users);

            builder.Entity<HR_Schedule>()
               .HasOptional(p => p.HR_MainSchedule);

            builder.Entity<HR_UserRoles>()
               .HasRequired(p => p.HR_Users);

            builder.Entity<HR_UserRoles>()
               .HasOptional(p => p.HR_Roles);

            
            builder.Entity<HR_Attendance>()
             .HasOptional(p => p.HR_Schedule);


            builder.Entity<HR_Timesheet>()
                .HasRequired(p => p.HR_Users);


            builder.Entity<HR_Timesheet_TagMapping>()
               .HasRequired(p => p.HR_Timesheet);

            builder.Entity<HR_Timesheet_TagMapping>()
               .HasRequired(p => p.HR_Timesheet_Tags);
        }
    }
}
