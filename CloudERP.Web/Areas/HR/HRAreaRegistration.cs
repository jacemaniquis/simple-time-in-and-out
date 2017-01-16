using System.Web.Mvc;

namespace CloudERP.Web.Areas.HR
{
    public class HRAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "HR";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "HR_default",
                "HR/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            context.MapRoute(
                name: "HR_ViewTimesheet",
                url: "HR/{controller}/View/{year}/{day}/{month}",
                defaults: new { controller = "Attendance", action = "Index", year = UrlParameter.Optional, day = UrlParameter.Optional, month = UrlParameter.Optional }
            );
        }
    }
}