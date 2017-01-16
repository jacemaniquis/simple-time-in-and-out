using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudERP.Context.Models;

namespace CloudERP.Web.Areas.HR.Models
{
    public class ViewUserModel
    {
        public List<HR_Users> Users { get; set; }
    }
}