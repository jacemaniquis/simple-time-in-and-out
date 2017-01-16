using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CloudERP.KioskWeb
{
    public static class Utils
    {
        public static DateTime GetTimezoneDateTime()
        {
            var serverTime = DateTime.Now;
            var localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, ConfigurationManager.AppSettings["Timezone"]);
            return localTime;
        }
    }
}