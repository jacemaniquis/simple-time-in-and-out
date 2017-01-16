using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CloudERP.Web
{
    public class DataResult : ContentResult
    {
        public object Data { get; set; }
    }
}