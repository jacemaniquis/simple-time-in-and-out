using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CloudERP.Web.Models.Api;

namespace CloudERP.Web
{
    public static class Utils
    {
        public static Result SetResult(Response response)
        {
            return (new Result()
            {
                Code = response,
                Description = response.ToString()
            });
        }


        public static string ContentAlertModal(string message, string redirectionUrl)
        {
            return "<script language='javascript' type='text/javascript'>alert('" + message + "');location.href='" + redirectionUrl + "'</script>";
        }

        public static object ViewBootstrapModal(Controller controller, string callbackUrl, string message)
        {
            return ViewBootstrapModal(controller, null, callbackUrl, string.Empty, message);
        }

        public static object ViewBootstrapModal(Controller controller, string callbackUrl, string title, string message)
        {
            return ViewBootstrapModal(controller, null, callbackUrl, title, message);
        }

        public static object ViewBootstrapModal(Controller controller, object model, string callbackUrl, string message)
        {
            return ViewBootstrapModal(controller, model, callbackUrl, string.Empty, message);
        }

        public static object ViewBootstrapModal(Controller controller, object model, string callbackUrl, string title, string message)
        {
            if (!string.IsNullOrEmpty(title))
            {
                controller.ViewBag.ModalTitle = title;
            }

            controller.ViewBag.ShowModal = true;
            controller.ViewBag.ModalMessage = message;

            controller.ViewBag.ModalCallbackUrl = callbackUrl;

            return model ?? string.Empty;
        }

        public static string ActionBootstrapModal(Controller controller, string actionName, string message)
        {
            return ActionBootstrapModal(controller, actionName, string.Empty, message);
        }

        public static string ActionBootstrapModal(Controller controller, string actionName, string title, string message)
        {
            if (!string.IsNullOrEmpty(title))
            {
                controller.TempData["ModalTitle"] = title;
            }

            controller.TempData["ShowModal"] = true;
            controller.TempData["ModalMessage"] = message;
            return actionName;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        // Verify a hash against a string. 
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input. 
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public static DateTime GetTimezoneDateTime()
        {
            var serverTime = DateTime.Now;
            var localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, ConfigurationManager.AppSettings["Timezone"]);
            return localTime;
        }
    }
}