using System.Web.Mvc;

namespace TeamCalendar.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult TeamCalendarLayout()
        {
            return View();
        }
    }
}