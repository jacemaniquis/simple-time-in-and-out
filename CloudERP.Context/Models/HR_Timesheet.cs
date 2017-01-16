using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Timesheet")]
    public class HR_Timesheet
    {
        [Key]
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Notes { get; set; }

        
        public virtual HR_Users HR_Users { get; set; }
    }
}
