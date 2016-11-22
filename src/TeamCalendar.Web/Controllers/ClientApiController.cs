using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI;
using TeamCalendar.Data;
using TeamCalendar.Web.Common;

namespace TeamCalendar.Web.Controllers
{
    [NoCache]
    public class ClientApiController : ApiControllerBase
    {
        [Route("client/GetLocalizedDate")]
        [HttpGet]
        public async Task<IHttpActionResult> GetLocalizedDate()
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            var myConfigCalendarProviderConfig = MyConfig.CalendarProviderConfig;

            return await Task.FromResult(Json(MyConfig.CalendarGridConfig
                .StartDate.AddHours(myConfigCalendarProviderConfig.TimeZone)
                .ToString("dddd dd. MMMM yyyy", myConfigCalendarProviderConfig.UiCulture)));
        }

        [Route("client/GetWeekNo")]
        [HttpGet]
        public async Task<IHttpActionResult> GetWeekNo()
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            var myConfigCalendarProviderConfig = MyConfig.CalendarProviderConfig;

            return await Task.FromResult(Json(myConfigCalendarProviderConfig
                .UiCulture.Calendar.GetWeekOfYear(MyConfig.CalendarGridConfig.StartDate
                        .AddHours(myConfigCalendarProviderConfig.TimeZone),
                    CalendarWeekRule.FirstFullWeek,
                    DayOfWeek.Monday)
                .ToString(CultureInfo.InvariantCulture)));
        }

        [Route("client/Ping")]
        [HttpGet]
        public IHttpActionResult Ping()
        {
            return Ok();
        }

        [Route("client/GetCalendarGridView")]
        [HttpGet]
        public async Task<IHttpActionResult> GetCalendarGridView()
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            var targetStartDate = DateTime.Today;

            var sessionConfig = MyConfig.CalendarGridConfig;
            sessionConfig.StartDate = targetStartDate;
            MyConfig.ConfigureGrid(sessionConfig);

            if (!await MyConfig.Populate())
            {
                return new StatusCodeResult(HttpStatusCode.InternalServerError, Request);
            }

            using (var sw = new StringWriter())
            {
                var writer = new HtmlTextWriter(sw);
                await MyConfig.CalendarGrid.Render(writer);
                return await Task.FromResult(Ok(sw.GetStringBuilder().ToString()));
            }
        }

        [Route("client/SetScreenResolution")]
        [HttpPost]
        public async Task<IHttpActionResult> SetScreenResolution(ScreenSize screenSize)
        {
            if (screenSize == null)
            {
                return new StatusCodeResult(HttpStatusCode.BadRequest, Request);
            }

            await MyConfig.SetScreenResolution(screenSize.Width, screenSize.Height);

            return Ok();
        }
    }
}