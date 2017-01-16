using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CloudERP.Context;
using CloudERP.Context.Models;
using CloudERP.Web.Areas.HR.Models;
using CloudERP.Web.Models.Api;
using Newtonsoft.Json;

namespace CloudERP.Web.Areas.HR.Controllers
{
    public class UsersController : Controller
    {
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Data()
        {
            var empRes = new EmployeeResult() { data = new List<Employee>() };

            try
            {
                var searchValue = Request["search[value]"].ToLower();
                var draw = Request["draw"] != null ? int.Parse(Request["draw"]) : 0;
                var start = Request["start"] != null ? int.Parse(Request["start"]) : 0;
                var length = Request["length"] != null ? int.Parse(Request["length"]) : 0;
                var orderColumn = Request["order[0][column]"];
                var orderBy = Request["order[0][dir]"];

                using (var context = new CloudERPDbContext())
                {
                    var result = context.HR_UserRoles.Where(p => !p.HR_Users.IsDeleted && p.To == null).ToList();

                    switch (orderBy)
                    {
                        case "desc":
                            switch (orderColumn)
                            {
                                case "1":
                                    result = result.OrderByDescending(p => p.HR_Users.MiddleName).ToList();
                                    break;
                                case "2":
                                    result = result.OrderByDescending(p => p.HR_Users.LastName).ToList();
                                    break;
                                case "3":
                                    result = result.OrderByDescending(p => p.HR_Roles.RoleName).ToList();
                                    break;
                                default:
                                    result = result.OrderByDescending(p => p.HR_Users.FirstName).ToList();
                                    break;
                            }
                            break;
                        default:
                            switch (orderColumn)
                            {
                                case "1":
                                    result = result.OrderBy(p => p.HR_Users.MiddleName).ToList();
                                    break;
                                case "2":
                                    result = result.OrderBy(p => p.HR_Users.LastName).ToList();
                                    break;
                                case "3":
                                    result = result.OrderBy(p => p.HR_Roles.RoleName).ToList();
                                    break;
                                default:
                                    result = result.OrderBy(p => p.HR_Users.FirstName).ToList();
                                    break;
                            }
                            break;
                    }

                    var predicate = PredicateBuilder.True<HR_UserRoles>();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        foreach (var item in searchValue.Split(' '))
                        {
                            var item1 = item;
                            predicate = predicate.And(p => p.HR_Users.FirstName != null && p.HR_Users.FirstName.ToLower().Contains(item1)
                                                || p.HR_Users.MiddleName != null && p.HR_Users.MiddleName.ToLower().Contains(item1)
                                                || p.HR_Users.LastName != null && p.HR_Users.LastName.ToLower().Contains(item1)
                                                || p.HR_Roles.RoleName != null && p.HR_Roles.RoleName.ToLower().Contains(item1));
                        }

                        result = result.Where(predicate.Compile()).ToList();
                    }


                    empRes.recordsTotal = result.Count;
                    empRes.recordsFiltered = result.Count;

                    result = result.Skip(start).Take(length).ToList();

                    empRes.draw = draw + 1;

                    foreach (var item in result)
                    {
                        empRes.data.Add(new Employee()
                        {
                            first_name = item.HR_Users.FirstName,
                            middle_name = item.HR_Users.MiddleName,
                            last_name = item.HR_Users.LastName,
                            role = item.HR_Roles.RoleName,
                            viewId = item.Id,
                            editId = item.Id,
                            deleteId = item.Id,
                            username = item.HR_Users.Username
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(empRes),
                ContentType = "application/json"
            };
        }



        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Edit(int id)
        {
            using (var context = new CloudERPDbContext())
            {
                var profile = context.HR_UserRoles.FirstOrDefault(p => p.Id == id);
                if (profile != null)
                {
                    var sched = context.HR_Schedule.FirstOrDefault(p => p.HR_Users.Id == profile.HR_Users.Id && p.To == null);
                    var model = new EditUserModel()
                    {
                        Id = profile.Id,
                        Username = profile.HR_Users.Username,
                        FirstName = profile.HR_Users.FirstName,
                        MiddleName = profile.HR_Users.MiddleName,
                        LastName = profile.HR_Users.LastName,
                        RoleId = profile.HR_Roles.Id,
                        MainScheduleId = sched != null ? sched.HR_MainSchedule.Id : 0
                    };
                    return View(model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Edit(EditUserModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.NewPassword != model.RetypeNewPassword)
                {
                    ModelState.AddModelError("", "Passwords are different");
                    return View(model);
                }

                using (var md5 = MD5.Create())
                using (var context = new CloudERPDbContext())
                {
                    var profile = context.HR_UserRoles.FirstOrDefault(p => p.Id == model.Id);
                    if (profile != null)
                    {
                        if (!string.IsNullOrEmpty(model.FirstName))
                        {
                            profile.HR_Users.FirstName = model.FirstName;
                        }

                        if (!string.IsNullOrEmpty(model.MiddleName))
                        {
                            profile.HR_Users.MiddleName = model.MiddleName;
                        }

                        if (!string.IsNullOrEmpty(model.LastName))
                        {
                            profile.HR_Users.LastName = model.LastName;
                        }

                        if (!string.IsNullOrEmpty(model.NewPassword))
                        {
                            profile.HR_Users.Password = Utils.GetMd5Hash(md5, model.NewPassword);
                        }

                        if (profile.HR_Roles.Id != model.RoleId)
                        {
                            profile.To = Utils.GetTimezoneDateTime();
                            context.SaveChanges();

                            var newRole = context.HR_Roles.FirstOrDefault(p => p.Id == model.RoleId);
                            context.HR_UserRoles.Add(new HR_UserRoles()
                            {
                                From = Utils.GetTimezoneDateTime(),
                                HR_Users = profile.HR_Users,
                                HR_Roles = newRole
                            });
                            context.SaveChanges();
                        }

                        var sched = context.HR_Schedule.FirstOrDefault(p => p.HR_Users.Id == profile.HR_Users.Id && p.To == null);
                        if (sched != null)
                        {
                            if (model.MainScheduleId != sched.HR_MainSchedule.Id)
                            {
                                var currentAttendance = context.HR_Attendance.FirstOrDefault(p => p.HR_Schedule.HR_Users.Id == profile.HR_Users.Id && p.TimeOut == null);

                                if (currentAttendance != null)
                                {
                                    ModelState.AddModelError("", "Can't update current user. User is currently logged in, please log out first.");
                                    return View(model);
                                }


                                sched.To = Utils.GetTimezoneDateTime();
                                context.SaveChanges();

                                if (model.MainScheduleId != 0)
                                {
                                    var newSched = context.HR_MainSchedule.FirstOrDefault(p => p.Id == model.MainScheduleId);
                                    context.HR_Schedule.Add(new HR_Schedule()
                                    {
                                        HR_MainSchedule = newSched,
                                        HR_Users = profile.HR_Users,
                                        From = Utils.GetTimezoneDateTime()
                                    });
                                    context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            if (model.MainScheduleId != 0)
                            {
                                var newSched = context.HR_MainSchedule.FirstOrDefault(p => p.Id == model.MainScheduleId);
                                context.HR_Schedule.Add(new HR_Schedule()
                                {
                                    HR_MainSchedule = newSched,
                                    HR_Users = profile.HR_Users,
                                    From = Utils.GetTimezoneDateTime()
                                });
                                context.SaveChanges();
                            }
                        }
                    }
                    context.SaveChanges();


                    return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Record Successfully Updated."));
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Add()
        {
            var model = new AddUserModel();
            using (var context = new CloudERPDbContext())
            {
                var defaultSched = context.HR_MainSchedule.FirstOrDefault(p => p.IsDefault);
                if (defaultSched != null)
                {
                    model.MainScheduleId = defaultSched.Id;
                }
            }
            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Add(AddUserModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var md5 = MD5.Create())
                    using (var context = new CloudERPDbContext())
                    {
                        var username = context.HR_Users.FirstOrDefault(p => p.Username == model.Username);
                        if (username != null)
                        {
                            ModelState.AddModelError("", "Username already existing");
                            return View(model);
                        }
                        else
                        {
                            var role = context.HR_Roles.FirstOrDefault(p => p.Id == model.RoleId);
                            if (role == null)
                            {
                                return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Invalid Role Values. Please contact Admin and Please try again."));
                            }


                            var mainSched = context.HR_MainSchedule.FirstOrDefault(p => p.Id == model.MainScheduleId);
                            if (model.MainScheduleId != 0)
                            {
                                if (mainSched == null)
                                {
                                    return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Invalid Main Schedule Values. Please contact Admin and Please try again."));
                                }
                            }

                            var employee = context.HR_Users.Add(new Context.Models.HR_Users()
                            {
                                FirstName = model.FirstName,
                                MiddleName = model.MiddleName,
                                LastName = model.LastName,
                                Password = Utils.GetMd5Hash(md5, "12345"),
                                Username = model.Username
                            });
                            context.SaveChanges();

                            if (mainSched != null)
                            {
                                context.HR_Schedule.Add(new HR_Schedule()
                                {
                                    From = Utils.GetTimezoneDateTime(),
                                    HR_Users = employee,
                                    HR_MainSchedule = mainSched,
                                });
                                context.SaveChanges();
                            }

                            context.HR_UserRoles.Add(new HR_UserRoles()
                            {
                                From = Utils.GetTimezoneDateTime(),
                                HR_Roles = role,
                                HR_Users = employee,
                            });
                            context.SaveChanges();

                            context.SITE_PendingTask.Add(new SITE_PendingTask()
                            {
                                HR_Users = employee,
                                PendingTask = "PasswordNomination"
                            });
                            context.SaveChanges();

                            return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Record Successfully Created."));

                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO LOGGING ERROR HANDLING
                    ModelState.AddModelError("", "Unexpected error occur");
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Delete()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Delete(int id)
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var profile = context.HR_UserRoles.FirstOrDefault(p => p.Id == id);
                    if (profile != null)
                    {
                        if (profile.HR_Roles.Id == 2)
                        {
                            var globalAdmins = context.HR_UserRoles.Where(p => p.HR_Roles.Id == 2);
                            if (globalAdmins.Count() <= 1)
                            {
                                return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "Can't delete last Global Admin account."));
                            }
                        }

                        profile.HR_Users.NfcRfidSerial = null;
                        profile.HR_Users.IsDeleted = true;
                        context.SaveChanges();
                    }
                }

                return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "Record Successfully Deleted."));
            }
            catch (Exception ex)
            {
                //TODO
                return Content("<script language='javascript' type='text/javascript'>alert('Unexpected Error occur.');location.href='/HR/Employee'</script>");
            }

        }


        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult AssignNfcCard(int id)
        {
            using (var context = new CloudERPDbContext())
            {
                var user = context.HR_UserRoles.FirstOrDefault(p => p.Id == id);
                if (user != null)
                {
                    context.SITE_PendingTask.Add(new SITE_PendingTask()
                    {
                        UserId = user.HR_Users.Id,
                        PendingTask = "AssignNfcCard"
                    });
                    context.SaveChanges();
                }
            }

            return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "Please proceed to Kiosk to nominate card."));
        }





        protected class EmployeeResult
        {
            public int draw { get; set; }

            public int recordsTotal { get; set; }

            public int recordsFiltered { get; set; }

            public List<Employee> data { get; set; }
        }

        protected class Employee
        {
            public string username { get; set; }

            public string first_name { get; set; }

            public string middle_name { get; set; }

            public string last_name { get; set; }

            public string role { get; set; }

            public int viewId { get; set; }

            public int editId { get; set; }

            public int deleteId { get; set; }
        }
    }
}