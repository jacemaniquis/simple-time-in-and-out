using System;
using System.Collections.Generic;

namespace CloudERP.Web.Areas.HR.Models
{
    public class AttendanceViewModel
    {
        public DateTime Date { get; set; }

        public List<UserRecordViewModel> UserRecords { get; set; }
    }

    public class UserRecordViewModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; }

        public DateTime TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        public string Remarks { get; set; }

        public string EditedBy { get; set; }
    }
}