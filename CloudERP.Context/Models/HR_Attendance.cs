using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Attendance")]
    public class HR_Attendance
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }


        public virtual HR_Schedule HR_Schedule { get; set; }
    }
}
