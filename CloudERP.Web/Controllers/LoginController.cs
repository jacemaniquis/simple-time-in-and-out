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
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var md5 = MD5.Create())
                    using (var context = new CloudERPDbContext())
                    {
                        var pw = Utils.GetMd5Hash(md5, model.Password);
                        var res = context.HR_Users.FirstOrDefault(p => p.Username == model.Username && p.Password == pw);
                        if (res != null)
                        {
                            var pending = context.SITE_PendingTask.FirstOrDefault(p => p.HR_Users.Id == res.Id && p.PendingTask == "PasswordNomination");

                            if (pending == null)
                            {
                                FormsAuthentication.SetAuthCookie(model.Username, false);
                                if (Url.IsLocalUrl(returnUrl))
                                {
                                    return Redirect(returnUrl);
                                }
                                return RedirectToAction("Index", "Dashboard");
                            }
                            else
                            {
                                TempData["Username"] = model.Username;
                                return RedirectToAction("PasswordNomination", "Pending");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid Username / Password");
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