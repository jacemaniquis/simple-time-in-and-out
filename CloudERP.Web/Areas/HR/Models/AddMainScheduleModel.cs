using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CloudERP.Web.Areas.HR.Models
{
    public class AddMainScheduleModel
    {
        [Required]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; }
        
        [Display(Name = "Time-In")]
        public string TimeIn { get; set; }
        
        [Display(Name = "Time-Out")]
        public string TimeOut { get; set; }

        [Display(Name = "Default")]
        public bool IsDefault { get; set; }
    }
}