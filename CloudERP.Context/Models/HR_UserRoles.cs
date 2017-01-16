using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_UserRoles")]
    public class HR_UserRoles
    {
        [Key]
        public int Id { get; set; }

        public DateTime From { get; set; }

        public DateTime? To { get; set; }



        public virtual HR_Users HR_Users { get; set; }

        public virtual HR_Roles HR_Roles { get; set; }
    }
}
