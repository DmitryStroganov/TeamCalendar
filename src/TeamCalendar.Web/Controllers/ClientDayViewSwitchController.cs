using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI;
using TeamCalendar.Web.Common;

namespace TeamCalendar.Web.Controllers
{
    [NoCache]
    public class ClientDayViewSwitchController : ApiControllerBase
    {
        [Route("client/GetSpecificCalendarGridView")]
        [HttpPost]
        public async Task<IHttpActionResult> GetSpecificCalendarGridView([FromBody] int? dateShift)
        {
            if ((MyConfig == null) || !MyConfig.IsConfigured)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            var targetStartDate = DateTime.Today;

            if (dateShift.HasValue)
            {
                var dateDiff = DateTime.Today - DateTime.Today.AddDays(dateShift.Value);

                if (Math.Abs(dateDiff.Days) <= MyConfig.CalendarGridConfig.MaxDaysShift)
                {
                    targetStartDate = DateTime.Today.AddDays(dateShift.Value);
                }
            }

            var sessionConfig = MyConfig.CalendarGridConfig;
            sessionConfig.StartDate = targetStartDate;
            MyConfig.ConfigureGrid(sessionConfig);

            await MyConfig.Populate();

            using (var sw = new StringWriter())
            {
                var writer = new HtmlTextWriter(sw);
                await MyConfig.CalendarGrid.Render(writer);
                return await Task.FromResult(Ok(sw.GetStringBuilder().ToString()));
            }
        }
    }
}