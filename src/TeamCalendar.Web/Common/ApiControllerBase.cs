using System.Web.Http;
using TeamCalendar.Common.Common;

namespace TeamCalendar.Web.Common
{
    public abstract class ApiControllerBase : ApiController
    {
        protected static CalendarAppConfig MyConfig => CalendarAppConfig.Instance.Value;
    }
}