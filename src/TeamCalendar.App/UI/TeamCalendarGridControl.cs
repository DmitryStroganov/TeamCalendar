using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using TeamCalendar.Data;

namespace TeamCalendar.Common.UI
{
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class TeamCalendarGridControl
    {
        public string ClientID { get; private set; }

        private IList<CalendarDay> DayInfo { get; set; }
        private IList<CalendarResource> CalendarResource { get; set; }

        private CalendarGridConfig Configuration { get; set; }

        private int Width { get; set; }

        public void Configure(CalendarGridConfig calendarGridConfig)
        {
            Configuration = calendarGridConfig;

            Configuration.StartDate = calendarGridConfig.StartDate;
            Configuration.EndDate = Configuration.StartDate.AddDays(Configuration.Days);

            Width = calendarGridConfig.Width > 0
                ? calendarGridConfig.Width
                : Configuration.RowHeaderWidth + (GetVisibleEnd.Hour - GetVisibleStart.Hour)*Configuration.CellWidth;
        }

        public void Populate(IList<CalendarInfo> calendarInformation, IList<CalendarResource> calendarResource)
        {
            CalendarResource = calendarResource;
            LoadEventsToDays(calendarInformation);
        }

        public Task<bool> Render(HtmlTextWriter output)
        {
            if (Configuration == null)
            {
                output.Write("<br/>");
                output.Write(HtmlTools.GetHtmlError("Invalid configuration: Team Calendar is not configured."));
                output.Write("<br/>");
                return Task.FromResult(false);
            }

            output.AddAttribute("id", ClientID);
            output.AddStyleAttribute("width", Width + "px");
            output.AddStyleAttribute("line-height", "1.2");
            output.RenderBeginTag("div");

            output.AddAttribute("cellspacing", "0");
            output.AddAttribute("cellpadding", "0");
            output.AddAttribute("border", "0");
            output.AddStyleAttribute("background-color", Configuration.HourNameBackColor);
            output.RenderBeginTag("table");

            output.RenderBeginTag("tr");
            output.AddStyleAttribute("width", Configuration.RowHeaderWidth + "px");
            output.RenderBeginTag("td");
            RenderCorners(output);
            output.RenderEndTag();
            RenderHeaderColumns(output);
            output.RenderEndTag();
            RenderRows(output);
            output.RenderEndTag();
            output.RenderEndTag();

            return Task.FromResult(true);
        }

        private void RenderEvents(CalendarDay calendarDay, HtmlTextWriter output)
        {
            if (calendarDay.Events.Count != 0)
            {
                output.AddStyleAttribute("position", "relative");
                output.AddStyleAttribute("height", calendarDay.MaxColumns()*Configuration.EventHeight - 1 + "px");
                output.AddStyleAttribute("overflow", "none");
                output.AddAttribute("unselectable", "on");
                output.RenderBeginTag("div");

                foreach (var ep in calendarDay.Events.Where(ep => ep.Start.Hour <= Configuration.BusinessEndsHour))
                {
                    RenderEvent(calendarDay, ep, output);
                }

                output.RenderEndTag();
            }
        }

        private void RenderEvent(CalendarDay calendarDay, CalendarEvent calendarEvent, HtmlTextWriter output)
        {
            var max = CellCount*Configuration.CellWidth;

            var dayVisibleStart = new DateTime(calendarDay.StartDate.Year,
                calendarDay.StartDate.Month,
                calendarDay.StartDate.Day,
                GetVisibleStart.Hour,
                0,
                0);
            var realBoxStart = calendarEvent.Start < dayVisibleStart ? dayVisibleStart : calendarEvent.Start;

            DateTime dayVisibleEnd;
            switch (GetVisibleEnd.Day)
            {
                case 1:
                    dayVisibleEnd = new DateTime(calendarDay.StartDate.Year,
                        calendarDay.StartDate.Month,
                        calendarDay.StartDate.Day,
                        GetVisibleEnd.Hour + 1,
                        0,
                        0);
                    break;
                default:
                    dayVisibleEnd =
                        new DateTime(calendarDay.StartDate.Year, calendarDay.StartDate.Month, calendarDay.StartDate.Day, GetVisibleEnd.Hour, 0, 0)
                            .AddDays(1);
                    break;
            }

            var realBoxEnd = calendarEvent.End > dayVisibleEnd ? dayVisibleEnd : calendarEvent.End;

            var left = (int) Math.Floor((realBoxStart - dayVisibleStart).TotalMinutes*Configuration.CellWidth/Configuration.CellDuration);
            var top = calendarEvent.Column.Number*Configuration.EventHeight - 1;
            var width = (int) Math.Floor((realBoxEnd - realBoxStart).TotalMinutes*Configuration.CellWidth/Configuration.CellDuration) - 3;
            var height = Configuration.EventHeight - 1;

            if (left > max)
            {
                return;
            }

            if (left + width > max - 2)
            {
                width = max - left - 2;
            }

            if (left < 0)
            {
                width += left;
                left = 0;
            }

            width = Math.Max(width, 2);
            output.AddAttribute("unselectable", "on");
            output.AddStyleAttribute("position", "absolute");
            output.AddStyleAttribute("left", left + "px");
            output.AddStyleAttribute("top", top + "px");
            output.AddStyleAttribute("width", width + "px");
            output.AddStyleAttribute("height", height + "px");
            output.AddStyleAttribute("border", "1px solid " + Configuration.EventBorderColor);

            var eventBackColor = Configuration.EventBackColor;

            if (!string.IsNullOrEmpty(calendarEvent.LegacyFreeBusyStatus))
            {
                switch (calendarEvent.LegacyFreeBusyStatus)
                {
                    case "OOF":
                        eventBackColor = Configuration.EventBackColor_OOF;
                        break;
                    case "Tentative":
                        eventBackColor = Configuration.EventBackColor_Tentative;
                        break;
                    case "Free":
                        eventBackColor = Configuration.EventBackColor_Free;
                        break;
                    default:
                        eventBackColor = Configuration.EventBackColor;
                        break;
                }
            }

            output.AddStyleAttribute("background-color", eventBackColor);
            output.AddStyleAttribute("white-space", "nowrap");
            output.AddStyleAttribute("overflow", "hidden");
            output.AddStyleAttribute("display", "block");
            output.AddStyleAttribute("padding-left", "1px");
            output.RenderBeginTag("div");

            output.AddStyleAttribute("font-family", Configuration.EventFontFamily);
            output.AddStyleAttribute("font-size", Configuration.EventFontSize);
            output.RenderBeginTag("div");

            output.Write(calendarEvent.Name.Length < Configuration.MaxEventTextLenght
                ? calendarEvent.Name
                : calendarEvent.Name.Substring(0, 22) + "...");
            output.Write(calendarEvent.Name);
            if (!string.IsNullOrEmpty(calendarEvent.Location))
            {
                output.Write("<br/><strong>{0}</strong>", calendarEvent.Location);
            }

            output.RenderEndTag();
            output.RenderEndTag();
        }

        private void RenderRows(HtmlTextWriter output)
        {
            if (DayInfo == null)
            {
                return;
            }

            var i = 0;

            foreach (var d in DayInfo)
            {
                var bgColor = i++%2 == 1 ? Configuration.BackgroundColor : Configuration.BackgroundColorAlt;

                output.RenderBeginTag("tr");

                RenderRowHeader(output, d, bgColor);
                RenderRowCells(output, d, bgColor);

                output.RenderEndTag();
            }
        }

        private void RenderRowCells(HtmlTextWriter output, CalendarDay calendarDay, string bgColor)
        {
            output.AddStyleAttribute("width", "1px");
            output.AddStyleAttribute("border-bottom", "1px solid black");
            output.AddStyleAttribute("background-color", GetCellColor(calendarDay.StartDate));
            output.AddStyleAttribute("valign", "top");
            output.AddStyleAttribute("unselectable", "on");
            output.RenderBeginTag("td");

            RenderEvents(calendarDay, output);

            output.RenderEndTag();

            var cellsToRender = GetVisibleEnd.Hour + 1;
            if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Show)
            {
                cellsToRender = CellCount;
            }

            for (var i = GetVisibleStart.Hour; i < cellsToRender; i++)
            {
                var thisCellWidth = Configuration.CellWidth;
                if (i == GetVisibleStart.Hour)
                {
                    thisCellWidth = Configuration.CellWidth - 1;
                }

                if (i == cellsToRender - 1)
                {
                    output.AddStyleAttribute("border-right", "1px solid " + Configuration.BorderColor);
                }

                output.AddStyleAttribute("border-bottom", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("width", thisCellWidth + "px");
                output.AddStyleAttribute("background-color", bgColor);

                output.RenderBeginTag("td");
                output.Write("<div unselectable='on' style='display:block; width:" + (thisCellWidth - 1) + "px; height:" +
                             (calendarDay.MaxColumns()*Configuration.EventHeight - 1) + "px; border-right: 1px solid " + Configuration.HourBorderColor +
                             ";' >&nbsp;</div>");
                output.RenderEndTag();
            }
        }

        private void RenderRowHeader(HtmlTextWriter output, CalendarDay calendarDay, string bgColor)
        {
            var height = calendarDay.MaxColumns()*Configuration.EventHeight - 1;

            if (!calendarDay.IsRoom)
            {
                output.AddStyleAttribute("width", Configuration.RowHeaderWidth - 1 + "px");
                output.AddStyleAttribute("border-right", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("border-left", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("border-bottom", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("background-color", bgColor);
                output.AddStyleAttribute("font-family", Configuration.HeaderFontFamily);
                output.AddStyleAttribute("font-size", Configuration.HeaderFontSize);
                output.AddStyleAttribute("color", Configuration.HeaderFontColor);
            }
            else
            {
                output.AddStyleAttribute("width", Configuration.RowHeaderWidth - 1 + "px");
                output.AddStyleAttribute("border-right", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("border-left", "2px solid " + Configuration.RoomHeaderBorderColor);
                output.AddStyleAttribute("border-bottom", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("background-color", bgColor);
                output.AddStyleAttribute("font-family", Configuration.HeaderFontFamily);
                output.AddStyleAttribute("font-size", Configuration.RoomHeaderHeaderFontSize);
                output.AddStyleAttribute("color", Configuration.RoomHeaderFontColor);
            }

            output.RenderBeginTag("td");

            output.Write("<div unselectable='on' style='margin-left:4px; height:" + height + "px; line-height:" + height + "px; overflow:hidden;'>");
            output.Write(calendarDay.ResourceName);
            output.Write("</div>");

            output.RenderEndTag();
        }

        private void RenderCorners(HtmlTextWriter output)
        {
            output.AddStyleAttribute("width", Configuration.RowHeaderWidth - 1 + "px");
            output.AddStyleAttribute("height", Configuration.HeaderHeight - 1 + "px");
            output.AddStyleAttribute("border-right", "1px solid " + Configuration.BorderColor);
            output.AddStyleAttribute("border-top", "1px solid " + Configuration.BorderColor);
            output.AddStyleAttribute("border-left", "1px solid " + Configuration.BorderColor);
            output.AddStyleAttribute("border-bottom", "1px solid " + Configuration.BorderColor);
            output.AddStyleAttribute("background-color", Configuration.HourNameBackColor);
            output.AddStyleAttribute("cursor", "default");
            output.AddAttribute("unselectable", "on");
            output.RenderBeginTag("div");
            output.RenderEndTag();
        }

        internal void RenderHeaderColumns(HtmlTextWriter output)
        {
            for (var i = GetVisibleStart.Hour; i < GetVisibleEnd.Hour + 1; i++)
            {
                var from = Configuration.StartDate.AddMinutes(Configuration.CellDuration*i);

                string text;

                if (Configuration.CellDuration < 60)
                {
                    text = $"<span style='color:#ccc'>{from.Minute:00}</span>";
                }
                else if (Configuration.CellDuration < 1440)
                {
                    text = $"{from.Hour:D2}";
                }
                else
                {
                    text = from.Day.ToString();
                }

                if (i == GetVisibleStart.Hour)
                {
                    output.AddAttribute("colspan", "2");
                }
                if (i == CellCount - 1)
                {
                    output.AddStyleAttribute("border-right", "1px solid " + Configuration.BorderColor);
                }
                output.AddStyleAttribute("border-top", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("border-bottom", "1px solid " + Configuration.BorderColor);
                output.AddStyleAttribute("width", Configuration.CellWidth + "px");
                output.AddStyleAttribute("height", Configuration.HeaderHeight - 1 + "px");
                output.AddStyleAttribute("overflow", "hidden");
                output.AddStyleAttribute("text-align", "center");
                output.AddStyleAttribute("background-color", Configuration.HourNameBackColor);
                output.AddStyleAttribute("font-family", Configuration.HourFontFamily);
                output.AddStyleAttribute("font-size", Configuration.HourFontSize);
                output.AddAttribute("unselectable", "on");
                output.AddStyleAttribute("-khtml-user-select", "none");
                output.AddStyleAttribute("-moz-user-select", "none");
                output.AddStyleAttribute("cursor", "default");
                output.RenderBeginTag("td");

                output.Write("<div unselectable='on' style='height:" + (Configuration.HeaderHeight - 1) + "px;border-right: 1px solid " +
                             Configuration.HourNameBorderColor + "; width:" + (Configuration.CellWidth - 1) + "px;overflow:hidden;'>");
                output.Write(text);
                output.Write("</div>");

                output.RenderEndTag();
            }
        }

        private void LoadEventsToDays(IList<CalendarInfo> calendarInformation)
        {
            if ((calendarInformation == null) || !calendarInformation.Any())
            {
                return;
            }
            if ((CalendarResource == null) || !CalendarResource.Any())
            {
                return;
            }

            var cEvents = calendarInformation.Select(cInfo => new CalendarEvent
            {
                PK = cInfo.EventID,
                Start = cInfo.StartTime,
                End = cInfo.EndTime,
                Name = cInfo.Subject,
                ResourceEmail = cInfo.ResourceEmail,
                Location = cInfo.Location,
                LegacyFreeBusyStatus = cInfo.Status
            }).OrderBy(c => c.Start).ThenBy(c => c.End).ToList();

            //separate rooms from humans
            var roomKeywords = Configuration.RoomKeywords.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

            DayInfo = new List<CalendarDay>();

            foreach (var resource in CalendarResource)
            {
                if (string.IsNullOrEmpty(resource.Email))
                {
                    continue;
                }

                var day = new CalendarDay
                {
                    StartDate = Configuration.StartDate,
                    EndDate = Configuration.EndDate,
                    ResourceName = resource.Name,
                    ResourceEmail = resource.Email
                };

                if (!string.IsNullOrEmpty(day.ResourceName))
                {
                    var s = day.ResourceName.Trim().ToLowerInvariant();

                    if (roomKeywords.Any(keyword => s.Contains(keyword.ToLowerInvariant())))
                    {
                        day.IsRoom = true;
                    }
                }

                DayInfo.Add(day);
            }

            //add humans 
            foreach (var day in DayInfo.Where(d => !d.IsRoom).OrderBy(d => d.ResourceName.Trim(), StringComparer.InvariantCultureIgnoreCase))
            {
                day.LoadEventData(cEvents);
            }

            //add rooms 
            foreach (var day in DayInfo.Where(d => d.IsRoom).OrderBy(d => d.ResourceName.Trim(), StringComparer.InvariantCultureIgnoreCase))
            {
                day.LoadEventData(cEvents);
            }
        }

        private int CellCount
        {
            get { return Configuration.Days*24*60/Configuration.CellDuration; }
        }

        private string GetCellColor(DateTime from)
        {
            var isBusiness = IsBusinessCell(from);

            return isBusiness ? Configuration.BackgroundColor : Configuration.NonBusinessBackColor;
        }

        private bool IsBusinessCell(DateTime from)
        {
            if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Hide)
            {
                return false;
            }

            if ((from.DayOfWeek == DayOfWeek.Saturday) || (from.DayOfWeek == DayOfWeek.Sunday))
            {
                return false;
            }

            if (Configuration.CellDuration < 720) // use hours
            {
                if ((from.Hour < Configuration.BusinessBeginsHour) || (from.Hour >= Configuration.BusinessEndsHour))
                {
                    return false;
                }
                return true;
            }
            return true;
        }

        private DateTime GetVisibleStart
        {
            get
            {
                var date = new DateTime(1900, 1, 1);

                if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Show)
                {
                    return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
                }

                var start = new DateTime(date.Year, date.Month, date.Day, Configuration.BusinessBeginsHour, 0, 0);

                if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Hide)
                {
                    return start;
                }

                if (DayInfo == null)
                {
                    return start;
                }

                if (TotalEvents == 0)
                {
                    return start;
                }

                foreach (var d in DayInfo)
                {
                    var boxStart = new DateTime(date.Year, date.Month, date.Day, d.BoxStart.Hour, d.BoxStart.Minute, d.BoxStart.Second);
                    if (boxStart < start)
                    {
                        start = boxStart;
                    }
                }

                return new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0);
            }
        }

        private DateTime GetVisibleEnd
        {
            get
            {
                var date = new DateTime(1900, 1, 1);

                if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Show)
                {
                    return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).AddDays(1);
                }

                DateTime end;
                if (Configuration.BusinessEndsHour == 24)
                {
                    end = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).AddDays(1);
                }
                else
                {
                    end = new DateTime(date.Year, date.Month, date.Day, Configuration.BusinessEndsHour, 0, 0);
                }

                if (Configuration.NonBusinessHours == NonBusinessHoursBehavior.Hide)
                {
                    return end;
                }

                if (DayInfo == null)
                {
                    return end;
                }

                if (TotalEvents == 0)
                {
                    return end;
                }

                foreach (var d in DayInfo)
                {
                    var addDay = (d.BoxEnd > DateTime.MinValue) && (d.BoxEnd.AddDays(-1) >= d.StartDate);

                    var boxEnd = new DateTime(date.Year, date.Month, date.Day, d.BoxEnd.Hour, d.BoxEnd.Minute, d.BoxEnd.Second);

                    if (addDay)
                    {
                        boxEnd = boxEnd.AddDays(1);
                    }

                    if (boxEnd > end)
                    {
                        end = boxEnd;
                    }
                }

                if (end.Minute != 0)
                {
                    end = end.AddHours(1);
                }

                return new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0);
            }
        }

        private int TotalEvents
        {
            get { return DayInfo.Count; }
        }
    }
}