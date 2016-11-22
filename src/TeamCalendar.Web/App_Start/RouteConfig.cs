using System.Web.Mvc;
using System.Web.Routing;

namespace TeamCalendar.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new {controller = "Home", action = "TeamCalendarLayout", id = UrlParameter.Optional}
            );
        }
    }
}