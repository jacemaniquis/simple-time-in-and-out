using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Timesheet_Tags")]
    public class HR_Timesheet_Tags
    {
        [Key]
        public int Id { get; set; }

        public string Tag { get; set; }

        public bool IsDeleted { get; set; }
    }
}
