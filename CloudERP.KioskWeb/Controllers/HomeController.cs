using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudERP.Context;
using CloudERP.KioskWeb.Models;

namespace CloudERP.KioskWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Index(string serial, bool isLogin)
        {
            var model = new LoginLogoutModel() { IsLogin = isLogin };

            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var user = context.HR_Users.FirstOrDefault(p => p.NfcRfidSerial == serial && !p.IsDeleted);
                    if (user != null)
                    {
                        if (isLogin)
                        {
                            var attendanceWithLoginRecord = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                            if (attendanceWithLoginRecord != null)
                            {
                                model.Name = user.FirstName;
                                model.Description = ConfigurationManager.AppSettings["UserIsAlreadyLoggedIn"];
                                model.Status = Status.Warning;
                            }
                            else
                            {
                                var sched = context.HR_Schedule.FirstOrDefault(p => p.HR_Users.Id == user.Id && p.To == null);
                                var record = new Context.Models.HR_Attendance()
                                {
                                    HR_Schedule = sched,
                                    TimeIn = Utils.GetTimezoneDateTime(),
                                };
                                context.HR_Attendance.Add(record);
                                context.SaveChanges();

                                model.Name = user.FirstName;
                                model.Description = ConfigurationManager.AppSettings["SuccessfulRequestLogin"];
                                model.Status = Status.Success;
                            }
                        }
                        else
                        {
                            var attendanceWithLoginRecord = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id && p.TimeOut == null);

                            if (attendanceWithLoginRecord == null)
                            {
                                model.Name = user.FirstName;
                                model.Description = ConfigurationManager.AppSettings["UserIsNotYetLoggedIn"];
                                model.Status = Status.Warning;
                            }
                            else
                            {
                                attendanceWithLoginRecord.TimeOut = Utils.GetTimezoneDateTime();
                                context.Entry(attendanceWithLoginRecord).State = System.Data.Entity.EntityState.Modified;
                                context.SaveChanges();

                                var duplicateRecords = context.HR_Attendance.Where(p =>
                                                                p.HR_Schedule != null && p.HR_Schedule.HR_Users.Id == user.Id &&
                                                                p.TimeOut == null);

                                if (duplicateRecords.Any())
                                {
                                    foreach (var record in duplicateRecords)
                                    {
                                        context.HR_Attendance.Remove(record);
                                        context.SaveChanges();
                                    }
                                }

                                model.Name = user.FirstName;
                                model.Description = ConfigurationManager.AppSettings["SuccessfulRequestLogout"];
                                model.Status = Status.Success;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                model.Description = ex.Message;
                model.Status = Status.Error;
            }

            return PartialView("~/Views/Shared/_ToastModal.cshtml", model);
        }


        [HttpPost]
        public ActionResult CheckCardAssigning()
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var pending = context.SITE_PendingTask.FirstOrDefault(p => p.PendingTask == "AssignNfcCard");
                    if (pending != null)
                    {
                        return Content(pending.Id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }


        public ActionResult AssignCard(int id)
        {
            var model = new AssignCardModel();

            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var pending = context.SITE_PendingTask.FirstOrDefault(p => p.Id == id);
                    if (pending != null)
                    {
                        var user = context.HR_Users.FirstOrDefault(p => p.Id == pending.UserId);
                        if (user != null)
                        {
                            model.Name = user.FirstName;
                            model.Id = pending.Id;
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return View(model);
        }

        [HttpPost]
        public ActionResult AssignCard(int id, string serial)
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var pending = context.SITE_PendingTask.FirstOrDefault(p => p.Id == id);
                    if (pending != null)
                    {
                        var user = context.HR_Users.FirstOrDefault(p => p.Id == pending.UserId);
                        if (user != null)
                        {
                            var nfcRecord = context.HR_Users.FirstOrDefault(p => p.NfcRfidSerial == serial);
                            if (nfcRecord != null)
                            {
                                nfcRecord.NfcRfidSerial = null;
                                context.SaveChanges();

                                user.NfcRfidSerial = serial;
                                context.SaveChanges();
                            }
                            else
                            {
                                user.NfcRfidSerial = serial;
                                context.SaveChanges();
                            }

                            context.SITE_PendingTask.Remove(pending);
                            context.SaveChanges();

                            return Content("true");
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        [HttpPost]
        public ActionResult AssignCardCancel(int id)
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var pending = context.SITE_PendingTask.FirstOrDefault(p => p.Id == id);
                    if (pending != null)
                    {
                        context.SITE_PendingTask.Remove(pending);
                        context.SaveChanges();
                        return RedirectToAction("Index");
                    }
                }

            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("Index");
        }








        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}