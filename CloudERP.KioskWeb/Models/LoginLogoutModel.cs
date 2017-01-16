using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudERP.KioskWeb.Models
{
    public class LoginLogoutModel
    {
        public Status Status { get; set; }

        public bool IsLogin { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
    }

    public enum Status
    {
        Success,
        Warning,
        Error
    }
}