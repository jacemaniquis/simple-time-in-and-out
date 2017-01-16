using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudERP.Web.Models
{
    public class ChromeModel
    {
        public bool IsLoggedIn { get; set; }

        public string LastTimeIn { get; set; }

        public string LastTimeOut { get; set; }

        public string TotalHours { get; set; }

        public string Name { get; set; }

        public int UserId { get; set; }
    }
}