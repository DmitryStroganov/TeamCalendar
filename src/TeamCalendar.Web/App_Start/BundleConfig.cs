using System.Web.Optimization;

namespace TeamCalendar.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-3.1.0.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/teamcalendar").Include("~/Scripts/TeamCalendarScripts.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/styles.css"));

#if DEBUG
            BundleTable.EnableOptimizations = false;
#endif
        }
    }
}