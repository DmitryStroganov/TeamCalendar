using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using TeamCalendar.Data;
using TeamCalendar.Web.Common;

namespace TeamCalendar.Web.Controllers
{
    [NoCache]
    public class ServerApiController : ApiControllerBase
    {
        [Route("server/GetConfig")]
        [HttpGet]
        public async Task<IHttpActionResult> GetConfig()
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NotImplemented, Request);
            }

            return await Task.FromResult(Json(MyConfig.CalendarGridConfig));
        }

        [Route("server/GetServerErrors")]
        [HttpGet]
        public async Task<IHttpActionResult> GetServerErrors()
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NotImplemented, Request);
            }

            if ((MyConfig.LastErrors == null) || !MyConfig.LastErrors.Any())
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            return await Task.FromResult(Json(MyConfig.LastErrors));
        }

        [Route("server/SetDate")]
        [HttpPost]
        public IHttpActionResult SetDate(ServerApiControllerConfig.SetDateConfig settings)
        {
            if (!string.Equals(MyConfig.ServerApiKey, settings.ApiKey, StringComparison.InvariantCultureIgnoreCase))
            {
                return new StatusCodeResult(HttpStatusCode.Unauthorized, Request);
            }

            if (settings.DateShift == 0)
            {
                return new StatusCodeResult(HttpStatusCode.BadRequest, Request);
            }

            MyConfig.CalendarGridConfig.StartDate = MyConfig.CalendarGridConfig.StartDate.AddDays(settings.DateShift);

            return Ok();
        }
    }
}