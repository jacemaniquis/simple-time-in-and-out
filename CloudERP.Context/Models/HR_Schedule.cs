using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Schedule")]
    public class HR_Schedule
    {
        [Key]
        public int Id { get; set; }

        public DateTime From { get; set; }

        public DateTime? To { get; set; }



        public virtual HR_Users HR_Users { get; set; }

        public virtual HR_MainSchedule HR_MainSchedule { get; set; }
    }
}
