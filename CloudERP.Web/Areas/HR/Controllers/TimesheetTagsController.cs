using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CloudERP.Web.Areas.HR.Controllers
{
    public class TimesheetTagsController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        public ActionResult Edit()
        {
            return View();
        }
    }
}