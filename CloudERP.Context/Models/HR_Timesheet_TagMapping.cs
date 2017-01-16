using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Timesheet_TagMapping")]
    public class HR_Timesheet_TagMapping
    {
        [Key]
        public int Id { get; set; }


        public virtual HR_Timesheet HR_Timesheet { get; set; }

        public virtual HR_Timesheet_Tags HR_Timesheet_Tags { get; set; }
    }
}
