using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CloudERP.Web.Areas.HR.Models;
using CloudERP.Context;
using CloudERP.Context.Models;
using Newtonsoft.Json;
using SpreadsheetLight;

namespace CloudERP.Web.Areas.HR.Controllers
{
    public class AttendanceController : Controller
    {
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Index(int? year, int? day, int? month)
        {
            var attendance = new AttendanceViewModel();

            if (year == null || day == null || month == null)
            {
                attendance.Date = Utils.GetTimezoneDateTime();
            }
            else
            {
                var dateValue = new DateTime(year ?? 0, month ?? 0, day ?? 0);
                attendance.Date = dateValue;
            }
            return View(attendance);
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Index(string date)
        {
            var attendance = new AttendanceViewModel();

            if (string.IsNullOrEmpty(date))
            {
                attendance.Date = Utils.GetTimezoneDateTime();
            }
            else
            {
                try
                {
                    var dt = date.Split('/');
                    var dateValue = new DateTime(int.Parse(dt[2]), int.Parse(dt[0]), int.Parse(dt[1]));
                    attendance.Date = dateValue;
                }
                catch (Exception ex)
                {
                    attendance.Date = Utils.GetTimezoneDateTime();
                }

            }
            return View(attendance);
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Range()
        {
            var attendance = new RangeAttendanceViewModel()
            {
                FirstDate = Utils.GetTimezoneDateTime(),
                LastDate = Utils.GetTimezoneDateTime()
            };
            return View(attendance);
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult Range(string firstdate, string lastdate)
        {
            var attendance = new RangeAttendanceViewModel();

            if (string.IsNullOrEmpty(firstdate) || string.IsNullOrEmpty(lastdate))
            {
                attendance.FirstDate = Utils.GetTimezoneDateTime();
                attendance.LastDate = Utils.GetTimezoneDateTime();
            }
            else
            {
                try
                {
                    attendance.FirstDate = Convert.ToDateTime(firstdate);
                    attendance.LastDate = Convert.ToDateTime(lastdate);
                }
                catch (Exception ex)
                {
                    attendance.FirstDate = Utils.GetTimezoneDateTime();
                    attendance.LastDate = Utils.GetTimezoneDateTime();
                }
            }

            return View(attendance);
        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public DataResult RangeData(string firstdate, string lastdate, bool isExport = false, string param = "")
        {
            var resData = new RangeTimesheet() { data = new List<Dictionary<string, string>>() };

            try
            {
                var qs = HttpUtility.ParseQueryString(param);
                var searchValue = qs["search[value]"] != null ? qs["search[value]"].ToLower() : Request["search[value]"].ToLower();
                var draw = qs["draw"] != null ? int.Parse(qs["draw"]) : Request["draw"] != null ? int.Parse(Request["draw"]) : 0;
                var start = qs["start"] != null ? int.Parse(qs["start"]) : Request["start"] != null ? int.Parse(Request["start"]) : 0;
                var length = qs["length"] != null ? int.Parse(qs["length"]) : Request["length"] != null ? int.Parse(Request["length"]) : 0;
                var orderColumn = qs["order[0][column]"] != null ? qs["order[0][column]"] : Request["order[0][column]"];
                var orderBy = qs["order[0][dir]"] != null ? qs["order[0][dir]"] : Request["order[0][dir]"];

                DateTime fDate;
                DateTime sDate;
                if (!DateTime.TryParse(firstdate, out fDate)) { return null; }
                if (!DateTime.TryParse(lastdate, out sDate)) { return null; }

                using (var context = new CloudERPDbContext())
                {
                    fDate = Convert.ToDateTime(fDate.ToString("MM/dd/yy"));
                    sDate = Convert.ToDateTime(sDate.ToString("MM/dd/yy"));

                    var attendance = new List<HR_Attendance>();
                    var attendancefullList = new List<HR_Attendance>();

                    if (User.IsInRole("Global Admin") || User.IsInRole("Admin"))
                    {
                        attendance = context.HR_Attendance.ToList()
                                            .Where(p => Convert.ToDateTime(p.TimeIn.ToString("MM/dd/yy")) >= fDate
                                                    && Convert.ToDateTime(p.TimeIn.ToString("MM/dd/yy")) <= sDate
                                                    && p.HR_Schedule != null)
                                            .ToList();
                        attendancefullList = attendance;
                    }
                    else
                    {

                        var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name);

                        if (user != null)
                        {
                            attendance = context.HR_Attendance.ToList()
                                            .Where(p => Convert.ToDateTime(p.TimeIn.ToString("MM/dd/yy")) >= fDate
                                                    && Convert.ToDateTime(p.TimeIn.ToString("MM/dd/yy")) <= sDate
                                                    && p.HR_Schedule != null
                                                    && p.HR_Schedule.HR_Users.Id == user.Id)
                                            .ToList();
                            attendancefullList = attendance;
                        }
                    }

                    attendance = attendance.GroupBy(p => p.HR_Schedule.HR_Users)
                                            .Select(p => p.First())
                                            .ToList();

                    var predicate = PredicateBuilder.True<HR_Attendance>();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        foreach (var item in searchValue.Split(' '))
                        {
                            var item1 = item;
                            predicate = predicate.And(p => p.HR_Schedule.HR_Users.FirstName != null && p.HR_Schedule.HR_Users.FirstName.ToLower().Contains(item1)
                                                || p.HR_Schedule.HR_Users.MiddleName != null && p.HR_Schedule.HR_Users.MiddleName.ToLower().Contains(item1)
                                                || p.HR_Schedule.HR_Users.LastName != null && p.HR_Schedule.HR_Users.LastName.ToLower().Contains(item1)
                                                || p.HR_Schedule.HR_MainSchedule.TimeIn != null && p.HR_Schedule.HR_MainSchedule.TimeIn.Value.ToString("h:mm tt").Contains(item1)
                                                || p.HR_Schedule.HR_MainSchedule.TimeOut != null && p.HR_Schedule.HR_MainSchedule.TimeOut.Value.ToString("h:mm tt").Contains(item1)
                                                || p.HR_Schedule.HR_MainSchedule.ScheduleNickname.ToLower().Contains(item1));
                        }

                        attendance = attendance.Where(predicate.Compile()).ToList();
                    }

                    switch (orderBy)
                    {
                        case "desc":
                            switch (orderColumn)
                            {
                                default:
                                    attendance = attendance.OrderByDescending(p => p.HR_Schedule.HR_Users.FirstName).ToList();
                                    break;
                            }
                            break;
                        default:
                            switch (orderColumn)
                            {
                                default:
                                    attendance = attendance.OrderBy(p => p.HR_Schedule.HR_Users.FirstName).ToList();
                                    break;
                            }
                            break;
                    }


                    foreach (var item in attendance)
                    {
                        var totalHours = new TimeSpan();
                        var totalDays = 0;
                        var dic = new Dictionary<string, string>();
                        var scheds = new List<HR_MainSchedule>();

                        foreach (var day in EachDay(fDate, sDate))
                        {
                            var user1 = item;
                            var userSheetForTheDay = context.HR_Attendance.Where(p => p.TimeIn.Year == day.Year
                                                            && p.TimeIn.Month == day.Month
                                                            && p.TimeIn.Day == day.Day
                                                            && p.HR_Schedule != null
                                                            && p.HR_Schedule.HR_Users.Id == user1.HR_Schedule.HR_Users.Id).OrderBy(p => p.Id).ToList();

                            foreach (var sheet in userSheetForTheDay)
                            {
                                var sched = scheds.LastOrDefault();
                                if (sched == null || sched.Id != sheet.HR_Schedule.HR_MainSchedule.Id)
                                {
                                    scheds.Add(sheet.HR_Schedule.HR_MainSchedule);
                                }

                                totalHours = totalHours + GetDateDifference(sheet.HR_Schedule.HR_MainSchedule.TimeIn, sheet.TimeIn, sheet.TimeOut);
                            }

                            var firstTimeIn = userSheetForTheDay.OrderBy(p => p.Id).FirstOrDefault();
                            var lastTimeIn = userSheetForTheDay.OrderBy(p => p.Id).LastOrDefault();

                            if (firstTimeIn != null && lastTimeIn != null)
                            {
                                totalDays = totalDays + 1;
                            }

                            dic.Add(day.ToString("MMMM_dd").ToLower(),
                                firstTimeIn == null && lastTimeIn == null ? "-" :
                                string.Format("{0} - {1}",
                                                firstTimeIn != null ? firstTimeIn.TimeIn.ToString("MM/dd/yy h:mm tt") : "N/A",
                                                lastTimeIn != null && lastTimeIn.TimeOut != null ? lastTimeIn.TimeOut.Value.ToString("MM/dd/yy h:mm tt") : "N/A"));
                        }

                        dic.Add("name", string.Format("{0} {1} {2}", item.HR_Schedule.HR_Users.FirstName, item.HR_Schedule.HR_Users.MiddleName, item.HR_Schedule.HR_Users.LastName));
                        dic.Add("total_hours", GetTotalForTheDay(totalHours));
                        dic.Add("schedule", GetMainSchedulesForRange(attendancefullList, scheds, item));
                        dic.Add("total_days", totalDays.ToString());

                        resData.data.Add(dic);

                    }

                    resData.recordsFiltered = resData.data.Count();
                    resData.recordsTotal = resData.data.Count();

                    resData.draw = draw + 1;

                    if (!isExport)
                    {
                        resData.data = resData.data.Skip(start).Take(length).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new DataResult()
            {
                Content = JsonConvert.SerializeObject(resData),
                ContentType = "application/json",
                Data = resData
            };
        }


        private string GetMainSchedulesForRange(List<HR_Attendance> attendance, List<HR_MainSchedule> scheds, HR_Attendance item)
        {
            var str = string.Empty;

            if (scheds.Count <= 1)
            {
                return item.HR_Schedule.HR_MainSchedule.TimeIn == null && item.HR_Schedule.HR_MainSchedule.TimeOut == null ? item.HR_Schedule.HR_MainSchedule.ScheduleNickname : string.Format("{0} - {1}",
                                        item.HR_Schedule.HR_MainSchedule.TimeIn != null ? item.HR_Schedule.HR_MainSchedule.TimeIn.Value.ToString("h:mm tt") : "N/A",
                                        item.HR_Schedule.HR_MainSchedule.TimeOut != null ? item.HR_Schedule.HR_MainSchedule.TimeOut.Value.ToString("h:mm tt") : "N/A");
            }

            foreach (var sched in scheds)
            {
                var firstDate = attendance.FirstOrDefault(p => p.HR_Schedule.HR_MainSchedule.Id == sched.Id && p.HR_Schedule.HR_Users.Id == item.HR_Schedule.HR_Users.Id);
                var lastDate = attendance.LastOrDefault(p => p.HR_Schedule.HR_MainSchedule.Id == sched.Id && p.HR_Schedule.HR_Users.Id == item.HR_Schedule.HR_Users.Id);

                if (firstDate != null && lastDate != null)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = str + string.Format("{0} - {1} ({2})", firstDate.TimeIn.ToString("dd MMM"), lastDate.TimeIn.ToString("dd MMM"),
                        firstDate.HR_Schedule.HR_MainSchedule.TimeIn == null && firstDate.HR_Schedule.HR_MainSchedule.TimeOut == null ? firstDate.HR_Schedule.HR_MainSchedule.ScheduleNickname : string.Format("{0} - {1}",
                                        firstDate.HR_Schedule.HR_MainSchedule.TimeIn != null ? firstDate.HR_Schedule.HR_MainSchedule.TimeIn.Value.ToString("h:mm tt") : "N/A",
                                        firstDate.HR_Schedule.HR_MainSchedule.TimeOut != null ? firstDate.HR_Schedule.HR_MainSchedule.TimeOut.Value.ToString("h:mm tt") : "N/A"));
                    }
                    else
                    {
                        str = str + string.Format(" | {0} - {1} ({2})", firstDate.TimeIn.ToString("dd MMM"), lastDate.TimeIn.ToString("dd MMM"),
                        firstDate.HR_Schedule.HR_MainSchedule.TimeIn == null && firstDate.HR_Schedule.HR_MainSchedule.TimeOut == null ? firstDate.HR_Schedule.HR_MainSchedule.ScheduleNickname : string.Format("{0} - {1}",
                                        firstDate.HR_Schedule.HR_MainSchedule.TimeIn != null ? firstDate.HR_Schedule.HR_MainSchedule.TimeIn.Value.ToString("h:mm tt") : "N/A",
                                        firstDate.HR_Schedule.HR_MainSchedule.TimeOut != null ? firstDate.HR_Schedule.HR_MainSchedule.TimeOut.Value.ToString("h:mm tt") : "N/A"));
                    }
                    
                }
            }

            return str;
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public DataResult Data(string date, bool isExport = false)
        {
            var resData = new Attendance() { data = new List<TableData>() };

            try
            {
                var searchValue = Request["search[value]"].ToLower();
                var draw = Request["draw"] != null ? int.Parse(Request["draw"]) : 0;
                var start = Request["start"] != null ? int.Parse(Request["start"]) : 0;
                var length = Request["length"] != null ? int.Parse(Request["length"]) : 0;
                var orderColumn = Request["order[0][column]"];
                var orderBy = Request["order[0][dir]"];

                DateTime selDate;
                if (!DateTime.TryParse(date, out selDate))
                {
                    return null;
                }

                using (var context = new CloudERPDbContext())
                {
                    var result = new List<HR_Attendance>();

                    if (User.IsInRole("Global Admin") || User.IsInRole("Admin"))
                    {
                        result = context.HR_Attendance.Where(p =>
                                        p.TimeIn.Year == selDate.Year
                                        && p.TimeIn.Month == selDate.Month
                                        && p.TimeIn.Day == selDate.Day
                                        && p.HR_Schedule != null).ToList();
                    }
                    else
                    {
                        var user = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name);

                        if (user != null)
                        {
                            result = context.HR_Attendance.Where(p =>
                                           p.TimeIn.Year == selDate.Year
                                           && p.TimeIn.Month == selDate.Month
                                           && p.TimeIn.Day == selDate.Day
                                           && p.HR_Schedule != null
                                           && p.HR_Schedule.HR_Users.Id == user.Id).ToList();
                        }
                    }

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
                                    result = result.OrderByDescending(p => p.HR_Schedule.HR_Users.FirstName).ToList();
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
                                    result = result.OrderBy(p => p.HR_Schedule.HR_Users.FirstName).ToList();
                                    break;
                            }
                            break;
                    }

                    var predicate = PredicateBuilder.True<HR_Attendance>();

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        foreach (var item in searchValue.Split(' '))
                        {
                            var item1 = item;
                            predicate = predicate.And(p => p.HR_Schedule.HR_Users.FirstName != null && p.HR_Schedule.HR_Users.FirstName.ToLower().Contains(item1)
                                                || p.HR_Schedule.HR_Users.MiddleName != null && p.HR_Schedule.HR_Users.MiddleName.ToLower().Contains(item1)
                                                || p.HR_Schedule.HR_Users.LastName != null && p.HR_Schedule.HR_Users.LastName.ToLower().Contains(item1)
                                                || p.TimeIn != null && p.TimeIn.ToString("h:mm tt").Contains(item1)
                                                || p.TimeOut != null && p.TimeOut.Value.ToString("h:mm tt").Contains(item1));
                        }

                        result = result.Where(predicate.Compile()).ToList();
                    }

                    resData.recordsTotal = result.Count;
                    resData.recordsFiltered = result.Count;

                    var completeData = result;
                    if (!isExport)
                    {
                        result = result.Skip(start).Take(length).ToList();
                    }

                    resData.draw = draw + 1;

                    foreach (var item in result)
                    {
                        if (item.HR_Schedule != null)
                        {
                            var item1 = item;
                            var userSheetForTheDay = completeData.Where(p => p.HR_Schedule.HR_Users.Id == item1.HR_Schedule.HR_Users.Id).OrderBy(p => p.Id);
                            var date2 = new TimeSpan();


                            foreach (var sheet in userSheetForTheDay)
                            {
                                date2 = date2 +
                                        GetDateDifference(sheet.HR_Schedule.HR_MainSchedule.TimeIn, sheet.TimeIn,
                                            sheet.TimeOut);
                            }


                            resData.data.Add(new TableData()
                            {
                                name = string.Format("{0} {1} {2}", item.HR_Schedule.HR_Users.FirstName, item.HR_Schedule.HR_Users.MiddleName, item.HR_Schedule.HR_Users.LastName),
                                time_in = item.TimeIn.ToString("MM/dd/yy h:mm tt"),
                                time_out = item.TimeOut?.ToString("MM/dd/yy h:mm tt") ?? string.Empty,
                                total = GetTotalDayAndHours(item.HR_Schedule.HR_MainSchedule.TimeIn, item.TimeIn, item.TimeOut),
                                overall_total = GetTotalForTheDay(date2),
                                main_schedule = item.HR_Schedule.HR_MainSchedule.TimeIn == null && item.HR_Schedule.HR_MainSchedule.TimeOut == null ? item.HR_Schedule.HR_MainSchedule.ScheduleNickname : string.Format("{0} - {1}",
                                            item.HR_Schedule.HR_MainSchedule.TimeIn != null ? item.HR_Schedule.HR_MainSchedule.TimeIn.Value.ToString("h:mm tt") : "N/A",
                                            item.HR_Schedule.HR_MainSchedule.TimeOut != null ? item.HR_Schedule.HR_MainSchedule.TimeOut.Value.ToString("h:mm tt") : "N/A")
                            });

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return new DataResult()
            {
                Content = JsonConvert.SerializeObject(resData),
                ContentType = "application/json",
                Data = resData
            };
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult ExportToExcel(string frmt, string callbackurl)
        {
            switch (frmt)
            {
                case "byrange":
                    var fdate = Request["firstdate"];
                    var ldate = Request["lastdate"];

                    using (Stream receiveStream = Request.InputStream)
                    {
                        using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            var body = readStream.ReadToEnd();
                            return Json(new { url = Url.Action("ExportByRange") + (Request.Url != null ? Request.Url.Query : string.Empty), callbackurl = callbackurl, firstdate = fdate, lastdate = ldate, param = body }, JsonRequestBehavior.AllowGet);
                        }
                    }
                case "byday":
                default:
                    var date = Request["date"];
                    return Json(new { url = Url.Action("ExportByDay") + (Request.Url != null ? Request.Url.Query : string.Empty), callbackurl = callbackurl }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult ExportByDay(string date)
        {
            var result = (Attendance)Data(date, true).Data;
            var document = new SLDocument();
            var columns = typeof(TableData).GetProperties();

            for (int x = 0; x < columns.Count(); x++)
            {
                document.SetCellValue(1, x + 1, columns[x].Name.ToUpper().Replace("_", " "));
            }

            for (int x = 0; x < result.data.Count(); x++)
            {
                document.SetCellValue(2 + x, 1, result.data[x].name);
                document.SetCellValue(2 + x, 2, result.data[x].time_in);
                document.SetCellValue(2 + x, 3, result.data[x].time_out);
                document.SetCellValue(2 + x, 4, result.data[x].total);
                document.SetCellValue(2 + x, 5, result.data[x].overall_total);
                document.SetCellValue(2 + x, 6, result.data[x].main_schedule);
            }

            var userCookie = new HttpCookie("dayuploadexcel", "1");
            userCookie.Expires.AddDays(365);
            HttpContext.Response.Cookies.Add(userCookie);

            var ms = new MemoryStream();
            document.SaveAs(ms);
            ms.Position = 0;
            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheet-ExportByDay-" + Utils.GetTimezoneDateTime().ToString("mmddyyHmmss") + ".xlsx");

        }

        [HttpPost]
        [Authorize(Roles = "Global Admin, Admin")]
        public ActionResult ExportByRange(string firstdate, string lastdate, string param)
        {
            var result = (RangeTimesheet)RangeData(firstdate, lastdate, true, param).Data;
            var document = new SLDocument();


            document.SetCellValue(1, 1, "NAME");
            document.SetCellValue(1, 2, "TOTAL HOURS");
            document.SetCellValue(1, 3, "MAIN SCHEDULE");
            document.SetCellValue(1, 4, "TOTAL DAYS");

            DateTime fDate;
            DateTime sDate;
            if (!DateTime.TryParse(firstdate, out fDate)) { return null; }
            if (!DateTime.TryParse(lastdate, out sDate)) { return null; }

            var colCtr = 4;
            var days = EachDay(fDate, sDate);
            foreach (var day in days)
            {
                document.SetCellValue(1, colCtr + 1, day.ToString("MMMM dd"));
                colCtr++;
            }

            for (int x = 0; x < result.data.Count(); x++)
            {
                document.SetCellValue(2 + x, 1, result.data[x]["name"]);
                document.SetCellValue(2 + x, 2, result.data[x]["total_hours"]);
                document.SetCellValue(2 + x, 3, result.data[x]["schedule"]);
                document.SetCellValue(2 + x, 4, result.data[x]["total_days"]);

                var dayCtr = 4;
                foreach (var day in days)
                {
                    document.SetCellValue(2 + x, dayCtr + 1, result.data[x][day.ToString("MMMM_dd").ToLower()]);
                    dayCtr++;
                }
            }

            var userCookie = new HttpCookie("rangeuploadexcel", "1");
            userCookie.Expires.AddDays(365);
            HttpContext.Response.Cookies.Add(userCookie);

            var ms = new MemoryStream();
            document.SaveAs(ms);
            ms.Position = 0;
            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheet-ExportByRange-" + Utils.GetTimezoneDateTime().ToString("mmddyyHmmss") + ".xlsx");
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

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }


        protected class RangeTimesheet
        {
            public int draw { get; set; }

            public int recordsTotal { get; set; }

            public int recordsFiltered { get; set; }

            public List<Dictionary<string, string>> data { get; set; }
        }

        protected class Attendance
        {
            public int draw { get; set; }

            public int recordsTotal { get; set; }

            public int recordsFiltered { get; set; }

            public List<TableData> data { get; set; }
        }

        protected class TableData
        {
            public string name { get; set; }

            public string time_in { get; set; }

            public string time_out { get; set; }

            public string total { get; set; }

            public string overall_total { get; set; }

            public string main_schedule { get; set; }
        }

        /*
        [HttpPost]
        [Authorize]
        public ActionResult Index(string date)
        {
            var timesheet = new TimesheetViewModel() { UserRecords = new List<UserRecordViewModel>() };

            if (string.IsNullOrEmpty(date))
            {
                timesheet.Date = DateTime.Now;
            }
            else
            {
                try
                {
                    var dt = date.Split('/');
                    var dateValue = new DateTime(int.Parse(dt[2]), int.Parse(dt[0]), int.Parse(dt[1]));
                    timesheet.Date = dateValue;
                }
                catch (Exception ex)
                {
                    timesheet.Date = DateTime.Now;
                }

            }

            using (var context = new CloudERPDbContext())
            {
                if (User.IsInRole("Admin"))
                {
                    var result = context.HR_Attendance.Where(p =>
                            p.TimeIn.Year == timesheet.Date.Year
                            && p.TimeIn.Month == timesheet.Date.Month
                            && p.TimeIn.Day == timesheet.Date.Day).ToList();

                    foreach (var item in result)
                    {
                        var employeeInfo = context.HR_Users.FirstOrDefault(p => p.Id == item.UserId);
                        if (employeeInfo != null)
                        {
                            var editedBy = context.HR_Users.FirstOrDefault(p => p.Id == item.EditedByUserId);
                            timesheet.UserRecords.Add(new UserRecordViewModel()
                            {
                                Id = item.Id,
                                UserId = employeeInfo.Id,
                                Name = employeeInfo.FirstName + " " + employeeInfo.LastName,
                                TimeIn = item.TimeIn,
                                TimeOut = item.TimeOut,
                                Remarks = item.Remarks,
                                EditedBy = editedBy != null ? editedBy.FirstName + " " + editedBy.LastName : "N/A"
                            });
                        }
                    }
                }
                else
                {
                    var employeeInfo = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name);
                    if (employeeInfo != null)
                    {
                        var result = context.HR_Attendance.Where(p =>
                                p.TimeIn.Year == timesheet.Date.Year
                                && p.TimeIn.Month == timesheet.Date.Month
                                && p.TimeIn.Day == timesheet.Date.Day
                                && p.UserId == employeeInfo.Id).ToList();

                        foreach (var item in result)
                        {
                            var editedBy = context.HR_Users.FirstOrDefault(p => p.Id == item.EditedByUserId);
                            timesheet.UserRecords.Add(new UserRecordViewModel()
                            {
                                Id = item.Id,
                                UserId = employeeInfo.Id,
                                Name = employeeInfo.FirstName + " " + employeeInfo.LastName,
                                TimeIn = item.TimeIn,
                                TimeOut = item.TimeOut,
                                Remarks = item.Remarks,
                                EditedBy = editedBy != null ? editedBy.FirstName + " " + editedBy.LastName : "N/A"
                            });
                        }
                    }
                }
            }

            return View(timesheet);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? recordId, int? employeeId)
        {
            using (var context = new CloudERPDbContext())
            {
                var profile = context.HR_Users.FirstOrDefault(p => p.Id == employeeId);

                if (profile != null)
                {
                    if (!User.IsInRole("Admin"))
                    {
                        if (User.Identity.Name != profile.Username)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    var record = context.HR_Attendance.FirstOrDefault(p => p.Id == recordId);

                    if (record != null)
                    {
                        var editedBy = context.HR_Users.FirstOrDefault(p => p.Id == record.EditedByUserId);
                        var model = new UserRecordViewModel()
                        {
                            Id = record.Id,
                            UserId = record.UserId,
                            TimeIn = record.TimeIn,
                            TimeOut = record.TimeOut,
                            Remarks = record.Remarks,
                            EditedBy = editedBy != null ? editedBy.FirstName + " " + editedBy.LastName : string.Empty
                        };

                        return View(model);
                    }
                }

                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(UserRecordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new CloudERPDbContext())
                {
                    var record = context.HR_Attendance.FirstOrDefault(p => p.Id == model.Id);
                    if (record != null)
                    {
                        var editedBy = context.HR_Users.FirstOrDefault(p => p.Username == User.Identity.Name);

                        record.TimeIn = model.TimeIn;
                        record.TimeOut = model.TimeOut;
                        record.Remarks = model.Remarks;
                        record.EditedByUserId = editedBy != null ? editedBy.Id : (int?)null;
                        context.SaveChanges();

                        return Content("<script language='javascript' type='text/javascript'>alert('Record Successfully Updated.');location.href='/'</script>");
                    }
                }
            }

            return View(model);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            using (var context = new CloudERPDbContext())
            {
                var record = context.HR_Attendance.FirstOrDefault(p => p.Id == id);

                if (record != null)
                {
                    return View(new UserRecordViewModel()
                    {
                        Id = Convert.ToInt32(id)
                    });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(UserRecordViewModel model)
        {
            using (var context = new CloudERPDbContext())
            {
                var record = context.HR_Attendance.FirstOrDefault(p => p.Id == model.Id);
                if (record != null)
                {
                    context.HR_Attendance.Remove(record);
                    context.SaveChanges();

                    return Content("<script language='javascript' type='text/javascript'>alert('Record Successfully Deleted.');location.href='/'</script>");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Add()
        {
            return View();
        }
        */
    }
}