using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudERP.Context;
using CloudERP.Web.Models;

namespace CloudERP.Web.Controllers
{
    public class ChromeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            var model = new ChromeModel();
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name && !p.IsDeleted);
                if (user != null)
                {
                    var attendanceWithLoginRecord =
                        context.HR_Attendance.FirstOrDefault(
                            p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord != null)
                    {
                        return RedirectToAction("Started");
                    }
                    else
                    {
                        var lastLogin = context.HR_Attendance.ToList().LastOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id);
                        var lastLogout = context.HR_Attendance.ToList().LastOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut != null);

                        model.IsLoggedIn = false;
                        model.LastTimeIn = lastLogin != null ? lastLogin.TimeIn.ToString("MM/dd/yy h:mm tt") : "N /A";
                        model.LastTimeOut = lastLogout != null && lastLogout.TimeOut != null ? lastLogout.TimeOut.Value.ToString("MM/dd/yy h:mm tt") : "N /A";
                        model.Name = user.FirstName;
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Logout", new { Area = string.Empty, ReturnUrl = Url.Action("Index") });
                }
            }

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Index(ChromeModel model)
        {
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name && !p.IsDeleted);
                if (user != null)
                {
                    var attendanceWithLoginRecord = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord == null)
                    {
                        var sched = context.HR_Schedule.FirstOrDefault(p => p.HR_Users.Id == user.Id && p.To == null);
                        context.HR_Attendance.Add(new Context.Models.HR_Attendance()
                        {
                            HR_Schedule = sched,
                            TimeIn = Utils.GetTimezoneDateTime(),
                        });
                        context.SaveChanges();

                        return RedirectToAction("Started");
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Logout", new { Area = string.Empty, ReturnUrl = Url.Action("Index") });
                }
            }
        }

        [Authorize]
        public ActionResult Started()
        {
            var model = new ChromeModel();

            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name && !p.IsDeleted);
                if (user != null)
                {
                    var attendanceWithLoginRecord = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord != null)
                    {
                        model.LastTimeIn = attendanceWithLoginRecord.TimeIn.ToString("MM/dd/yy h:mm tt");
                        model.TotalHours =
                            GetTotalDayAndHours(attendanceWithLoginRecord.HR_Schedule.HR_MainSchedule.TimeIn,
                                attendanceWithLoginRecord.TimeIn, attendanceWithLoginRecord.TimeOut);

                        model.Name = user.FirstName;
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Logout", new { Area = string.Empty, ReturnUrl = Url.Action("Index") });
                }
            }

            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Started(ChromeModel model)
        {
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name && !p.IsDeleted);
                if (user != null)
                {
                    var attendanceWithLoginRecord = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord != null)
                    {
                        attendanceWithLoginRecord.TimeOut = Utils.GetTimezoneDateTime();
                        context.Entry(attendanceWithLoginRecord).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Index", "Logout", new { Area = string.Empty, ReturnUrl = Url.Action("Index") });
                }
            }
        }





        private string GetTotalForTheDay(TimeSpan result)
        {
            if (result.Days > 0)
            {
                return string.Format("{0}:{1}", ((result.Days * 24) + result.Hours).ToString("0"), result.Minutes.ToString("00"));
            }
            else
            {
                return string.Format("{0}:{1}", result.Hours.ToString("0"), result.Minutes.ToString("00"));
            }
        }

        private string GetTotalDayAndHours(DateTime? mainSchedTime, DateTime startDate, DateTime? endDate)
        {
            var result = GetDateDifference(mainSchedTime, startDate, endDate);

            if (result.Hours < 0 || result.Minutes < 0)
            {
                return string.Format("0:0");
            }
            else
            {
                return GetTotalForTheDay(result);
            }
        }

        private TimeSpan GetDateDifference(DateTime? mainSchedTime, DateTime startDate, DateTime? endDate)
        {
            if (endDate == null)
            {
                endDate = Utils.GetTimezoneDateTime();
            }

            startDate = Convert.ToDateTime(startDate.ToString("MM/dd/yy h:mm tt"));
            endDate = Convert.ToDateTime(endDate.Value.ToString("MM/dd/yy h:mm tt"));

            if (mainSchedTime != null)
            {
                var mainStartDate = Convert.ToDateTime(startDate.ToString("MM/dd/yy") + " " + mainSchedTime.Value.ToString("h:mm tt"));

                int compare = DateTime.Compare(mainStartDate, startDate);

                if (compare == 0 || compare > 0)
                {
                    startDate = mainStartDate;
                }
            }

            var eDate = Convert.ToDateTime(endDate);
            var result = eDate.Subtract(startDate);

            return result;
        }
    }
}