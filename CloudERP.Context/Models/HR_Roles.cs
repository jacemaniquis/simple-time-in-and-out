using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Context.Models
{
    [Table("HR_Roles")]
    public class HR_Roles
    {
        [Key]
        public int Id { get; set; }

        public string RoleName { get; set; }
    }
}
