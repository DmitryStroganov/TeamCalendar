using System;

namespace TeamCalendar.Common.UI
{
    public static class HtmlTools
    {
        public static string GetHtmlError(string errMsg)
        {
            return $"<div style='color:red'>{errMsg.Replace("\n", "<br/>")}</div>";
        }

        public static string GetHtmlError(Exception ex)
        {
            if (ex.InnerException == null)
            {
                return $"<div style='color:red'>{ex.Message.Replace("\n", "<br/>")}</div>";
            }

            return $"<div style='color:red'>{ex.InnerException.Message.Replace("\n", "<br/>")} : {ex.Message.Replace("\n", "<br/>")}</div>";
        }
    }
}