using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("SITE_PendingTask")]
    public class SITE_PendingTask
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("HR_Users")]
        public int UserId { get; set; }

        public string PendingTask { get; set; }





        public virtual HR_Users HR_Users { get; set; }
    }
}
