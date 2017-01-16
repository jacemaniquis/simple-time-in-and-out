using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CloudERP.Web.Areas.HR.Models
{
    public class AddUserModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Required]
        [Display(Name = "Main Schedule")]
        public int MainScheduleId { get; set; }
    }
}