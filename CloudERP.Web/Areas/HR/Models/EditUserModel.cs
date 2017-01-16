using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudERP.Web.Areas.HR.Models
{
    public class EditUserModel
    {
        public int Id { get; set; }

        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Main Schedule")]
        public int MainScheduleId { get; set; }

        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Display(Name = "Retype New Password")]
        public string RetypeNewPassword { get; set; }


    }
}
