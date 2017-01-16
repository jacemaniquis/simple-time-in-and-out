using System.Web;
using System.Web.Optimization;

namespace CloudERP.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        //"~/Scripts/jquery-{version}.js"));
                        "~/Scripts/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                         "~/Content/bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));




            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/site.css",
                     //"~/Content/daterangepicker.css",
                     "~/Content/bootstrap-datepicker.standalone.min.css",
                     "~/Content/bootstrap-datepicker3.standalone.min.css",
                     "~/Content/timeline.css",
                     "~/Content/sb-admin-2-v2.css",
                     "~/Content/metisMenu.min.css",
                     "~/Content/font-awesome.min.css",
                     "~/Content/dataTables.bootstrap.css",
                     "~/Content/bootstrap-timepicker.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                      "~/Scripts/metisMenu.min.js",
                      "~/Scripts/raphael-min.js",
                      "~/Scripts/morris.min.js",
                      "~/Scripts/sb-admin-2.js",
                       "~/Scripts/moment.min.js",
                      //"~/Scripts/jquery.daterangepicker.js",
                      "~/Scripts/bootstrap-datepicker.min.js",
                      "~/Scripts/jquery.dataTables.min.js",
                      "~/Scripts/dataTables.bootstrap.min.js",
                      "~/Scripts/bootstrap-timepicker.min.js"));

        }
    }
}
