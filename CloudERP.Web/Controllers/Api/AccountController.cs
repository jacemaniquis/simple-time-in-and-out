using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;
using CloudERP.Web.Models.Api;
using CloudERP.Context;
using CloudERP.Context.Models;

namespace CloudERP.Web.Controllers.Api
{
    public class AccountController : ApiController
    {
        /*
        [HttpGet]
        [Route("api/dtr/authenticate/{username}/{password}")]
        public Result Authenticate(string username, string password)
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    using (var md5Hash = MD5.Create())
                    {
                        var pword = Utils.GetMd5Hash(md5Hash, password);
                        if (context.HR_Users.FirstOrDefault(p => p.Username == username && p.Password == pword) != null)
                        {
                            return Utils.SetResult(Response.Authorized);
                        }
                        else
                        {
                            return Utils.SetResult(Response.NotAuthorized);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Utils.SetResult(Response.FailedRequest);
            }
        }

        [HttpPost]
        [Route("api/dtr/GetProfileInfo")]
        public GetProfileInfoResult GetProfileInfo([FromBody]Profile prof)
        {
            var result = new GetProfileInfoResult();
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var profile = context.HR_Users.FirstOrDefault(p => p.Username == prof.username);
                    if (profile != null)
                    {
                        result.Code = Response.SuccessfulRequest;
                        result.Description = Response.SuccessfulRequest.ToString();
                        result.FirstName = profile.FirstName;
                        result.LastName = profile.LastName;
                        result.Role = profile.HR_Roles.RoleName;
                    }
                    else
                    {
                        var serial = context.HR_NfcRfidMapping.FirstOrDefault(p => p.Serial == prof.serial);
                        if (serial != null)
                        {
                            var user = context.HR_Users.FirstOrDefault(p => p.HR_NfcRfidMapping == serial);
                            if (user != null)
                            {
                                result.Code = Response.SuccessfulRequest;
                                result.Description = Response.SuccessfulRequest.ToString();
                                result.FirstName = user.FirstName;
                                result.LastName = user.LastName;
                                result.Role = user.HR_Roles.RoleName;
                            }
                            else
                            {
                                result.Code = Response.UserNotExist;
                                result.Description = Response.UserNotExist.ToString();
                            }
                        }
                        else
                        {
                            result.Code = Response.UserNotExist;
                            result.Description = Response.UserNotExist.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = Response.FailedRequest;
                result.Description = Response.FailedRequest.ToString();
            }

            return result;
        }

        [HttpPost]
        [Route("api/dtr/AssignSerial")]
        public Result AssignSerial([FromBody] Profile prof)
        {
            try
            {
                using (var context = new CloudERPDbContext())
                {
                    var profile = context.HR_Users.FirstOrDefault(p => p.Username == prof.username);
                    if (profile != null)
                    {
                        var serial = context.HR_NfcRfidMapping.FirstOrDefault(p => p.Serial == prof.serial);
                        if (serial != null)
                        {
                            profile.HR_NfcRfidMapping = serial;
                            context.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                        }
                        else
                        {
                            var ser = new HR_NfcRfidMapping()
                            {
                                Serial = prof.serial
                            };
                            context.HR_NfcRfidMapping.Add(ser);
                            context.SaveChanges();

                            profile.HR_NfcRfidMapping = ser;
                            context.Entry(profile).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                        }
                        return Utils.SetResult(Response.SuccessfulRequest);
                    }
                    else
                    {
                        return Utils.SetResult(Response.UserNotExist);
                    }
                }
            }
            catch (Exception ex)
            {
                return Utils.SetResult(Response.FailedRequest);
            }
        }

        [HttpPost]
        [Route("api/dtr/Login")]
        public Result Login([FromBody] Profile prof)
        {
            try
            {
                var employee = GetEmployee(prof);
                using (var context = new CloudERPDbContext())
                {
                    if (employee == null)
                    {
                        return Utils.SetResult(Response.NoResult);
                    }

                    var attendanceWithLoginRecord = context.HR_Attendance
                                            .FirstOrDefault(p => p.UserId == employee.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord != null)
                    {
                        return Utils.SetResult(Response.UserIsAlreadyLoggedIn);
                    }
                    else
                    {
                        context.HR_Attendance.Add(new HR_Attendance()
                        {
                            UserId = employee.Id,
                            TimeIn = DateTime.Now,
                        });
                        context.SaveChanges();

                        return Utils.SetResult(Response.SuccessfulRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                return Utils.SetResult(Response.FailedRequest);
            }
        }

        [HttpPost]
        [Route("api/dtr/Logout")]
        public Result Logout([FromBody] Profile prof)
        {
            try
            {
                var employee = GetEmployee(prof);
                using (var context = new CloudERPDbContext())
                {
                    if (employee == null)
                    {
                        return Utils.SetResult(Response.NoResult);
                    }

                    var attendanceWithLoginRecord = context.HR_Attendance
                                            .FirstOrDefault(p => p.UserId == employee.Id && p.TimeOut == null);

                    if (attendanceWithLoginRecord == null)
                    {
                        return Utils.SetResult(Response.UserIsNotYetLoggedIn);
                    }
                    else
                    {
                        attendanceWithLoginRecord.TimeOut = DateTime.Now;
                        context.Entry(attendanceWithLoginRecord).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();

                        return Utils.SetResult(Response.SuccessfulRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                return Utils.SetResult(Response.FailedRequest);
            }
        }


        private HR_Users GetEmployee(Profile prof)
        {
            HR_Users employee = null;
            using (var context = new CloudERPDbContext())
            {

                if (!string.IsNullOrEmpty(prof.serial))
                {
                    var rfid = context.HR_NfcRfidMapping.FirstOrDefault(p => p.Serial == prof.serial);
                    if (rfid != null)
                    {
                        var res = context.HR_Users.FirstOrDefault(p => p.Id == rfid.Id);
                        if (res != null)
                        {
                            employee = res;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(prof.username))
                {
                    var res = context.HR_Users.FirstOrDefault(p => p.Username == prof.username);
                    if (res != null)
                    {
                        employee = res;
                    }
                }
            }
            return employee;
        }
        */
    }
}
