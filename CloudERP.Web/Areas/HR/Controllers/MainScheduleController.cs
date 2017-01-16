using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CloudERP.Context;
using CloudERP.Context.Models;
using CloudERP.Web.Areas.HR.Models;
using Newtonsoft.Json;

namespace CloudERP.Web.Areas.HR.Controllers
{
    public class MainScheduleController : Controller
    {

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Index()
        {
            return View();
        }



        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Data()
        {
            var resData = new DataResult() { data = new List<Sched>() };

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
                    var result = context.HR_MainSchedule.Where(p => !p.IsDeleted).ToList();

                    switch (orderBy)
                    {
                        case "desc":
                            switch (orderColumn)
                            {
                                case "1":
                                    result = result.OrderByDescending(p => p.TimeIn).ToList();
                                    break;
                                case "2":
                                    result = result.OrderByDescending(p => p.TimeOut).ToList();
                                    break;
                                default:
                                    result = result.OrderByDescending(p => p.ScheduleNickname).ToList();
                                    break;
                            }
                            break;
                        default:
                            switch (orderColumn)
                            {
                                case "1":
                                    result = result.OrderBy(p => p.TimeIn).ToList();
                                    break;
                                case "2":
                                    result = result.OrderBy(p => p.TimeOut).ToList();
                                    break;
                                default:
                                    result = result.OrderBy(p => p.ScheduleNickname).ToList();
                                    break;
                            }
                            break;
                    }

                    var predicate = PredicateBuilder.True<HR_MainSchedule>();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        foreach (var item in searchValue.Split(' '))
                        {
                            var item1 = item;
                            predicate = predicate.And(p => p.ScheduleNickname != null && p.ScheduleNickname.ToLower().Contains(item1)
                                                || p.TimeIn != null && p.TimeIn.Value.ToString("h:mm tt").Contains(item1)
                                                || p.TimeOut != null && p.TimeOut.Value.ToString("h:mm tt").Contains(item1));
                        }

                        result = result.Where(predicate.Compile()).ToList();
                    }


                    resData.recordsTotal = result.Count;
                    resData.recordsFiltered = result.Count;

                    result = result.Skip(start).Take(length).ToList();

                    resData.draw = draw + 1;

                    foreach (var item in result)
                    {
                        resData.data.Add(new Sched()
                        {
                            nickname = item.ScheduleNickname,
                            time_in = item.TimeIn != null ? item.TimeIn.Value.ToString("h:mm tt") : "N/A",
                            time_out = item.TimeOut != null ? item.TimeOut.Value.ToString("h:mm tt") : "N/A",
                            is_default = item.IsDefault.ToString(),
                            viewId = item.Id,
                            editId = item.Id,
                            deleteId = item.Id,
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(resData),
                ContentType = "application/json"
            };
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Add()
        {
            return View();
        }


        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Add(AddMainScheduleModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new CloudERPDbContext())
                {
                    if (model.TimeIn == null && model.TimeOut == null)
                    {
                        var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeIn == null && p.TimeOut == null && !p.IsDeleted);
                        if (res != null)
                        {
                            return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-in and Time-out."));
                        }
                        else
                        {
                            context.HR_MainSchedule.Add(new HR_MainSchedule()
                            {
                                ScheduleNickname = model.Nickname,
                                IsDefault = model.IsDefault,
                                TimeIn = null,
                                TimeOut = null,
                            });
                        }
                    }
                    else if (model.TimeIn == null)
                    {
                        var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeIn == null && p.TimeOut != null && !p.IsDeleted);
                        if (res != null)
                        {
                            return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-in."));
                        }
                        else
                        {
                            context.HR_MainSchedule.Add(new HR_MainSchedule()
                            {
                                ScheduleNickname = model.Nickname,
                                IsDefault = model.IsDefault,
                                TimeIn = null,
                                TimeOut = DateTime.Parse(model.TimeOut)
                            });
                        }
                    }
                    else if (model.TimeOut == null)
                    {
                        var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeOut == null && p.TimeIn != null && !p.IsDeleted);
                        if (res != null)
                        {
                            return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-out."));
                        }
                        else
                        {
                            context.HR_MainSchedule.Add(new HR_MainSchedule()
                            {
                                ScheduleNickname = model.Nickname,
                                IsDefault = model.IsDefault,
                                TimeIn = DateTime.Parse(model.TimeIn),
                                TimeOut = null
                            });
                        }
                    }
                    else
                    {
                        context.HR_MainSchedule.Add(new HR_MainSchedule()
                        {
                            ScheduleNickname = model.Nickname,
                            IsDefault = model.IsDefault,
                            TimeIn = DateTime.Parse(model.TimeIn),
                            TimeOut = DateTime.Parse(model.TimeOut)
                        });
                    }

                    if (model.IsDefault)
                    {
                        var curDefault = context.HR_MainSchedule.Where(p => p.IsDefault);
                        foreach (var item in curDefault)
                        {
                            item.IsDefault = false;
                        }
                    }
                    context.SaveChanges();

                    return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Record Successfully Created."));
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Edit(int id)
        {
            using (var context = new CloudERPDbContext())
            {
                var data = context.HR_MainSchedule.FirstOrDefault(p => p.Id == id);
                if (data != null)
                {
                    var model = new EditMainScheduleModel()
                    {
                        Id = data.Id,
                        Nickname = data.ScheduleNickname,
                        IsDefault = data.IsDefault,
                        TimeIn = data.TimeIn != null ? data.TimeIn.Value.ToString("h:mm tt") : null,
                        TimeOut = data.TimeOut != null ? data.TimeOut.Value.ToString("h:mm tt") : null
                    };
                    return View(model);
                }
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Edit(EditMainScheduleModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new CloudERPDbContext())
                {
                    var data = context.HR_MainSchedule.FirstOrDefault(p => p.Id == model.Id);
                    if (data != null)
                    {
                        if (model.TimeIn == null && model.TimeOut == null)
                        {
                            var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeIn == null && p.TimeOut == null && !p.IsDeleted && p.Id != data.Id);
                            if (res != null)
                            {
                                return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-in and Time-out."));
                            }
                            else
                            {
                                data.ScheduleNickname = model.Nickname;
                                data.IsDefault = model.IsDefault;
                                data.TimeIn = null;
                                data.TimeOut = null;
                            }
                        }
                        else if (model.TimeIn == null)
                        {
                            var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeIn == null && p.TimeOut != null && !p.IsDeleted && p.Id != data.Id);
                            if (res != null)
                            {
                                return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-in."));
                            }
                            else
                            {
                                data.ScheduleNickname = model.Nickname;
                                data.IsDefault = model.IsDefault;
                                data.TimeIn = null;
                                data.TimeOut = DateTime.Parse(model.TimeOut);
                            }
                        }
                        else if (model.TimeOut == null)
                        {
                            var res = context.HR_MainSchedule.FirstOrDefault(p => p.TimeOut == null && p.TimeIn != null && !p.IsDeleted && p.Id != data.Id);
                            if (res != null)
                            {
                                return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "There is already schedule with no Time-out."));
                            }
                            else
                            {
                                data.ScheduleNickname = model.Nickname;
                                data.IsDefault = model.IsDefault;
                                data.TimeIn = DateTime.Parse(model.TimeIn);
                                data.TimeOut = null;
                            }
                        }
                        else
                        {
                            data.ScheduleNickname = model.Nickname;
                            data.IsDefault = model.IsDefault;
                            data.TimeIn = DateTime.Parse(model.TimeIn);
                            data.TimeOut = DateTime.Parse(model.TimeOut);
                        }

                        if (data.IsDefault)
                        {
                            var curDefault = context.HR_MainSchedule.Where(p => p.IsDefault);
                            foreach (var item in curDefault)
                            {
                                item.IsDefault = false;
                            }
                        }
                        context.SaveChanges();

                        return View(Utils.ViewBootstrapModal(this, model, Url.Action("Index"), "Record Successfully Updated."));
                    }
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
                    var data = context.HR_MainSchedule.FirstOrDefault(p => p.Id == id);
                    var user = context.HR_Schedule.FirstOrDefault(p => p.HR_MainSchedule.Id == data.Id && !p.HR_Users.IsDeleted);
                    if (user != null)
                    {
                        return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "Can't delete record. There is a User using this schedule."));
                    }

                    if (data != null)
                    {
                        data.IsDeleted = true;
                        context.SaveChanges();
                    }

                    return RedirectToAction(Utils.ActionBootstrapModal(this, "Index", "Record Successfully Deleted."));
                }
            }
            catch (Exception ex)
            {
                //TODO
                return Content("<script language='javascript' type='text/javascript'>alert('Unexpected Error occur.');location.href='/HR/EmployeeMainSchedule'</script>");
            }
        }


        protected class DataResult
        {
            public int draw { get; set; }

            public int recordsTotal { get; set; }

            public int recordsFiltered { get; set; }

            public List<Sched> data { get; set; }
        }

        protected class Sched
        {
            public string nickname { get; set; }

            public string time_in { get; set; }

            public string time_out { get; set; }

            public string is_default { get; set; }

            public int viewId { get; set; }

            public int editId { get; set; }

            public int deleteId { get; set; }
        }
    }
}