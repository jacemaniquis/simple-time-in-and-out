using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CloudERP.Context;
using CloudERP.Web.Models;

namespace CloudERP.Web.Controllers
{
    public class PendingController : Controller
    {

        public ActionResult PasswordNomination()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PasswordNomination(PasswordNominationModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.Password != model.PasswordRetype)
                    {
                        ModelState.AddModelError("", "Passwords are different");
                        return View(model);
                    }

                    var username = TempData["Username"].ToString();
                    using (var md5 = MD5.Create())
                    using (var context = new CloudERPDbContext())
                    {
                        var profile = context.HR_Users.FirstOrDefault(p => p.Username == username);
                        if (profile != null)
                        {
                            profile.Password = Utils.GetMd5Hash(md5, model.Password);
                            context.SaveChanges();

                            var pending = context.SITE_PendingTask.FirstOrDefault(p => p.HR_Users.Id == profile.Id && p.PendingTask == "PasswordNomination");
                            context.SITE_PendingTask.Remove(pending);
                            context.SaveChanges();

                            FormsAuthentication.SetAuthCookie(profile.Username, false);
                            return RedirectToAction("Index", "Dashboard");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO LOGGING ERROR HANDLING
                }
            }

            return View(model);
        }
    }
}