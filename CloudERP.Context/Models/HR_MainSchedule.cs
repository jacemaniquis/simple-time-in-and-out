using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_MainSchedule")]
    public class HR_MainSchedule
    {
        [Key]
        public int Id { get; set; }
        
        public string ScheduleNickname { get; set; }

        public DateTime? TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        public bool IsDefault { get; set; }
        
        public bool IsDeleted { get; set; }
    }
}
