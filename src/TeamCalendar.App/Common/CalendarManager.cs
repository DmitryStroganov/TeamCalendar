using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using TeamCalendar.Data;

namespace TeamCalendar.Common.Common
{
    public static class CalendarManager
    {
        private static readonly string CacheKeyMain = string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}",
            Assembly.GetExecutingAssembly().GetName().Name,
            Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId,
            HttpRuntime.AppDomainAppId);

        private static readonly string UserAliasCachekey = CacheKeyMain + ":useralias";

        public static Dictionary<string, string> ResolveUserAccounts(ITeamCalendarUserDataResolver userDataResolver, string strTeamList)
        {
            if (string.IsNullOrWhiteSpace(strTeamList))
            {
                throw new ArgumentNullException(nameof(strTeamList));
            }

            var accountAliasMap = WebCacheHelper.Get<Dictionary<string, string>>(UserAliasCachekey);

            if ((accountAliasMap != null) && accountAliasMap.Any())
            {
                return accountAliasMap;
            }

            accountAliasMap = new Dictionary<string, string>();

            short cntUserNotFound = 0;

            foreach (var strUser in strTeamList.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var userId = strUser.Trim();
                if (string.IsNullOrEmpty(userId))
                {
                    continue;
                }

                //by AD id
                if ((userId.IndexOf(' ') == -1) && (userId.IndexOf('@') == -1))
                {
                    var userEml = userDataResolver.ResolveEmailByUsername(userId);

                    if (string.IsNullOrEmpty(userEml))
                    {
                        cntUserNotFound++;
                        accountAliasMap[$"unresolved_{cntUserNotFound}"] = "N/A: " + userId;
                    }
                    else
                    {
                        accountAliasMap[userEml] = userDataResolver.ResolveAccountDisplayNameByEmail(userId);
                    }

                    continue;
                }

                //by full name
                if ((userId.IndexOf(' ') != -1) && (userId.IndexOf('@') == -1))
                {
                    var userEml = userDataResolver.ResolveEmailByUsername(userId);

                    if (string.IsNullOrEmpty(userEml))
                    {
                        cntUserNotFound++;
                        accountAliasMap[$"unresolved_{cntUserNotFound}"] = "N/A: " + userId;
                    }
                    else
                    {
                        accountAliasMap[userEml] = userId;
                    }

                    continue;
                }

                //by email
                if ((userId.IndexOf(' ') == -1) && (userId.IndexOf('@') != -1))
                {
                    var userName = userDataResolver.ResolveAccountDisplayNameByEmail(userId);

                    if (string.IsNullOrEmpty(userName))
                    {
                        cntUserNotFound++;
                        accountAliasMap[$"unresolved_{cntUserNotFound}@"] = "N/A: " + userId;
                    }
                    else
                    {
                        accountAliasMap[userId] = userName;
                    }
                }
            }

            WebCacheHelper.Set(accountAliasMap, UserAliasCachekey, DateTime.UtcNow.AddHours(1).TimeOfDay.TotalSeconds);

            return accountAliasMap;
        }

        public static IEnumerable<CalendarInfo> ResolveCalendarData(ITeamCalendarDataProvider calendarDataProvider, DateTime startDate, IList<string> userAccounts)
        {
            if ((userAccounts == null) || !userAccounts.Any())
            {
                throw new InvalidOperationException("no user account data found.");
            }

            const ushort CacheTimeout = 60;
            var cacheKey = $"{CacheKeyMain}:{calendarDataProvider.GetHashCode()}:{startDate.ToFileTimeUtc()}";

            List<CalendarInfo> cInfoList = null;
            cInfoList = WebCacheHelper.Get<List<CalendarInfo>>(cacheKey);

            if (cInfoList != null)
            {
                return cInfoList;
            }

            cInfoList = new List<CalendarInfo>();

            //fetch
            foreach (var userEmail in userAccounts)
            {
                try
                {
                    var calendarInfos = calendarDataProvider.GetCalendarInfo(userEmail,
                        startDate,
                        new DateTime(startDate.Year, startDate.Month, startDate.Day).AddDays(1).AddSeconds(-1));

                    if (calendarInfos != null)
                    {
                        cInfoList.AddRange(calendarInfos.ToList());
                    }
                }
                catch (Exception ex)
                {
                    TraceTool.Instance.Value.TraceMessageFormat(
                        "Unable to retrive calendar information for '{0}': '{1}'",
                        userEmail,
                        ex.Message);

                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["TeamCalendar.DiagnisticMode"]) &&
                        ConfigurationManager.AppSettings["TeamCalendar.DiagnisticMode"].ToLower().Equals("true"))
                    {
                        throw new InvalidOperationException(
                            $"{DateTime.UtcNow.ToShortTimeString()} Unable to retrive calendar information for '{userEmail}': '{ex.Message}'");
                    }

                    break;
                }
            }

            //sort / merge
            if (cInfoList.Any())
            {
                var listMerge = (from c in cInfoList
                    group c by new {c.ResourceEmail, c.Location, c.StartTime, c.EndTime}
                    into g
                    select g.Aggregate(delegate(CalendarInfo current, CalendarInfo next)
                    {
                        current.Subject += "; " + next.Subject;
                        return current;
                    })).ToList();


                WebCacheHelper.Set(listMerge, cacheKey, CacheTimeout);
                return listMerge;
            }

            return Enumerable.Empty<CalendarInfo>();
        }
    }
}