using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TeamCalendar.Common.UI;
using TeamCalendar.Data;

namespace TeamCalendar.Common.Common
{
    public class CalendarAppConfig
    {
        private const string DefaultCalendarDataProvider = "TeamCalendar.CalendarDataProvider.Test.dll";
        private const string DefaultUserDataResolver = "TeamCalendar.CalendarDataProvider.Test.dll";

        public static readonly Lazy<CalendarAppConfig> Instance =
            new Lazy<CalendarAppConfig>(() => new CalendarAppConfig());

        public readonly DateTime DefaultStartDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month,
            DateTime.UtcNow.Day, 07, 0, 0);

        private CalendarAppConfig()
        {
            TraceTool.InstanceInitializer.Value.Invoke(new TraceTool.TraceToolSettings
            {
                Enabled = GetConfigValueOrDefault("TeamCalendar.DiagnisticMode", false),
                TraceDiag = true,
                TraceWarnings = true,
                TraceUnhandledExceptions = true,
                DateTimeFormat = "HH:mm:ss.fffff"
            });

            LastErrors = new ConcurrentBag<string>();

            CalendarProviderConfig = new CalendarServiceConfig
            {
                TimeZone = 1,
                UiCulture = new CultureInfo("en-GB")
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TeamCalendar.UICultureName"]))
                {
                    CalendarProviderConfig.UiCulture =
                        new CultureInfo(ConfigurationManager.AppSettings["TeamCalendar.UICultureName"]);
                }

                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TeamCalendar.TimeZoneNumber"]))
                {
                    var timeZone = 0;

                    if (int.TryParse(ConfigurationManager.AppSettings["TeamCalendar.TimeZoneNumber"],
                        NumberStyles.Integer, CalendarProviderConfig.UiCulture, out timeZone))
                    {
                        CalendarProviderConfig.TimeZone = timeZone;
                    }
                }

                AssignOrThrow(cfg => cfg.TeamList, ConfigurationManager.AppSettings["TeamCalendar.TeamList"],
                    () => new InvalidOperationException("TeamCalendar.TeamList not defined"));

                AssignOrThrow(cfg => cfg.RoomKeywords, ConfigurationManager.AppSettings["TeamCalendar.RoomKeywords"],
                    () => new InvalidOperationException("TeamCalendar.RoomKeywords not defined"));

                AssignOrThrow(cfg => cfg.ExchangeUrl, ConfigurationManager.AppSettings["TeamCalendar.ExchangeUrl"],
                    () => new InvalidOperationException("TeamCalendar.ExchangeUrl not defined"));

                AssignOrThrow(cfg => cfg.AccountUser, ConfigurationManager.AppSettings["TeamCalendar.AccountUser"],
                    () => new InvalidOperationException("TeamCalendar.AccountUser not defined"));

                AssignOrThrow(cfg => cfg.AccountPassword,
                    ConfigurationManager.AppSettings["TeamCalendar.AccountPassword"],
                    () => new InvalidOperationException("TeamCalendar.AccountPassword not defined"));

                AssignOrThrow(cfg => cfg.AccountDomain, ConfigurationManager.AppSettings["TeamCalendar.AccountDomain"],
                    () => new InvalidOperationException("TeamCalendar.AccountDomain not defined"));
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(HtmlTools.GetHtmlError("Unable to initialize TeamCalendar: check configuratiuon."));
                LastErrors.Add(HtmlTools.GetHtmlError(ex));
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TeamCalendar.CalendarDataProvider"]))
                {
                    CalendarDataProvider =
                        PluginLoader.LoadSingleFrom<ITeamCalendarDataProvider>(HttpRuntime.BinDirectory +
                                                                               DefaultCalendarDataProvider);
                }
                else
                {
                    CalendarDataProvider =
                        PluginLoader.LoadSingleFrom<ITeamCalendarDataProvider>(HttpRuntime.BinDirectory +
                                                                               ConfigurationManager.AppSettings[
                                                                                   "TeamCalendar.CalendarDataProvider"]);
                }

                if (CalendarDataProvider == null)
                {
                    throw new InvalidOperationException("CalendarDataProvider not initialized.");
                }

                if (typeof(ITeamCalendarUserDataResolver).IsAssignableFrom(CalendarDataProvider.GetType()))
                {
                    UserDataResolver = (ITeamCalendarUserDataResolver) CalendarDataProvider;
                }
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(HtmlTools.GetHtmlError($"Unable to load CalendarDataProvider: '{ConfigurationManager.AppSettings["TeamCalendar.CalendarDataProvider"]}'"));
                LastErrors.Add(HtmlTools.GetHtmlError(ex));
                return;
            }

            if (UserDataResolver == null)
            {
                try
                {
                    if (
                        string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TeamCalendar.CalendarUserDataResolver"]))
                    {
                        UserDataResolver =
                            PluginLoader.LoadSingleFrom<ITeamCalendarUserDataResolver>(HttpRuntime.BinDirectory +
                                                                                       DefaultUserDataResolver);
                    }
                    else
                    {
                        UserDataResolver =
                            PluginLoader.LoadSingleFrom<ITeamCalendarUserDataResolver>(HttpRuntime.BinDirectory +
                                                                                       ConfigurationManager.AppSettings[
                                                                                           "TeamCalendar.CalendarUserDataResolver"
                                                                                       ]);
                    }
                }
                catch (Exception ex)
                {
                    TraceTool.Instance.Value.TraceException(ex);
                    LastErrors.Add(HtmlTools.GetHtmlError($"Unable to load CalendarUserDataResolver: '{ConfigurationManager.AppSettings["TeamCalendar.CalendarUserDataResolver"]}'"));
                    LastErrors.Add(HtmlTools.GetHtmlError(ex));
                    return;
                }
            }

            if (UserDataResolver == null)
            {
                throw new InvalidOperationException("CalendarUserDataResolver not initialized.");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TeamCalendar.CryptoProvider"]))
                {
                    CryptoProvider =
                        PluginLoader.LoadSpecificFrom<ICryptoProvider>(ConfigurationManager.AppSettings["TeamCalendar.CryptoProvider"], HttpRuntime.BinDirectory);

                    if ((CryptoProvider != null) && CalendarProviderConfig.AccountPassword.StartsWith("=="))
                    {
                        var encodedPwd = CalendarProviderConfig.AccountPassword.Substring(2);
                        CalendarProviderConfig.AccountPassword =
                            Encoding.UTF8.GetString(CryptoProvider.Decode(Convert.FromBase64String(encodedPwd), CryptoProvider.GetPublicKey()));
                    }
                }
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(
                    HtmlTools.GetHtmlError(
                        $"Unable to load CryptoProvider: '{ConfigurationManager.AppSettings["TeamCalendar.CryptoProvider"]}'"));
                LastErrors.Add(HtmlTools.GetHtmlError(ex));
                return;
            }

            try
            {
                CalendarDataProvider.Initialize(CalendarProviderConfig);

                CalendarGridConfig = new CalendarGridConfig
                {
                    HoverBoxEnabled = GetConfigValueOrDefault("TeamCalendar.HoverBoxEnabled", true),
                    DateShiftEnabled = GetConfigValueOrDefault("TeamCalendar.DateShiftEnabled", false),
                    HourNameBackColor = GetConfigValueOrDefault("TeamCalendar.HourNameBackColor", "#FFFFFF"),
                    HourNameBorderColor = GetConfigValueOrDefault("TeamCalendar.HourNameBorderColor", "#394D63"),
                    EventBorderColor = GetConfigValueOrDefault("TeamCalendar.EventBorderColor", "#000000"),
                    EventBackColor = GetConfigValueOrDefault("TeamCalendar.EventBackColor", "#98FB98"),
                    EventBackColor_OOF = GetConfigValueOrDefault("TeamCalendar.EventBackColor_OOF", "#F08080"),
                    EventBackColor_Tentative = GetConfigValueOrDefault("TeamCalendar.EventBackColor_Tentative", "#F8F8FF"),
                    EventBackColor_Free = GetConfigValueOrDefault("TeamCalendar.EventBackColor_Free", "#cccccc"),
                    DurationBarVisible = GetConfigValueOrDefault("TeamCalendar.DurationBarVisible", true),
                    HoverColor = GetConfigValueOrDefault("TeamCalendar.HoverColor", "#FFED95"),
                    BackgroundColor = GetConfigValueOrDefault("TeamCalendar.BackgroundColor", "#DCDCDC"),
                    BackgroundColorAlt = GetConfigValueOrDefault("TeamCalendar.BackgroundColorAlt", "#F5F5F5"),
                    HeaderFontFamily = GetConfigValueOrDefault("TeamCalendar.HeaderFontFamily", "Tahoma"),
                    HeaderFontColor = GetConfigValueOrDefault("TeamCalendar.HeaderFontColor", "#000000"),
                    HeaderFontSize = GetConfigValueOrDefault("TeamCalendar.HeaderFontSize", "10pt"),
                    RoomHeaderFontColor = GetConfigValueOrDefault("TeamCalendar.RoomHeaderFontColor", "#FF5805"),
                    RoomHeaderHeaderFontSize = GetConfigValueOrDefault("TeamCalendar.RoomHeaderHeaderFontSize", "12pt"),
                    RoomHeaderBorderColor = GetConfigValueOrDefault("TeamCalendar.RoomHeaderBorderColor", "#FF5805"),
                    HourFontFamily = GetConfigValueOrDefault("TeamCalendar.HourFontFamily", "Tahoma"),
                    HourFontSize = GetConfigValueOrDefault("TeamCalendar.HourFontSize", "10pt"),
                    HourBorderColor = GetConfigValueOrDefault("TeamCalendar.HourBorderColor", "#EAD098"),
                    EventFontFamily = GetConfigValueOrDefault("TeamCalendar.EventFontFamily", "Tahoma"),
                    EventFontSize = GetConfigValueOrDefault("TeamCalendar.EventFontSize", "11px"),
                    NonBusinessBackColor = GetConfigValueOrDefault("TeamCalendar.NonBusinessBackColor", "#FFF4BC"),
                    NonBusinessHours = NonBusinessHoursBehavior.Hide,
                    BorderColor = GetConfigValueOrDefault("TeamCalendar.BorderColor", "#000000"),
                    BusinessBeginsHour = GetConfigValueOrDefault("TeamCalendar.BusinessBeginsHour", 7),
                    BusinessEndsHour = GetConfigValueOrDefault("TeamCalendar.BusinessEndsHour", 18),
                    CellDuration = GetConfigValueOrDefault("TeamCalendar.CellDuration", 60),
                    CellWidth = GetConfigValueOrDefault("TeamCalendar.CellWidth", 120),
                    RowHeaderWidth = GetConfigValueOrDefault("TeamCalendar.RowHeaderWidth", 240),
                    EventHeight = GetConfigValueOrDefault("TeamCalendar.EventHeight", 30),
                    HeaderHeight = GetConfigValueOrDefault("TeamCalendar.HeaderHeight", 25),
                    MaxEventTextLenght = GetConfigValueOrDefault("TeamCalendar.MaxEventTextLenght", 50),
                    Days = GetConfigValueOrDefault("TeamCalendar.Days", 1),
                    RoomKeywords = CalendarProviderConfig.RoomKeywords,
                    TimeZone = CalendarProviderConfig.TimeZone,
                    StartDate = DateTime.Today,
                    MaxDaysShift = GetConfigValueOrDefault("TeamCalendar.MaxDaysShift", 2)
                };

                CalendarGrid = new TeamCalendarGridControl();

                IsConfigured = true;
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(HtmlTools.GetHtmlError("Failed to initialize Calendar."));
                LastErrors.Add(HtmlTools.GetHtmlError(ex));
            }

            ServerApiKey = GetConfigValueOrDefault("TeamCalendar.ServerApiKey", new Guid().ToString("D"));
        }

        private ITeamCalendarDataProvider CalendarDataProvider { get; }
        private ITeamCalendarUserDataResolver UserDataResolver { get; }
        private ICryptoProvider CryptoProvider { get; }

        public ScreenSize ScreenSize { get; private set; }

        public bool IsConfigured { get; private set; }

        public TeamCalendarGridControl CalendarGrid { get; }
        public CalendarGridConfig CalendarGridConfig { get; }
        public CalendarServiceConfig CalendarProviderConfig { get; }

        public ConcurrentBag<string> LastErrors { get; }
        public string ServerApiKey { get; }

        private T GetConfigValueOrDefault<T>(string settingName, T valueDefault)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[settingName]))
            {
                return valueDefault;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFromInvariantString(ConfigurationManager.AppSettings[settingName]);
        }

        private void AssignOrThrow(
            Expression<Func<CalendarServiceConfig, object>> propInfo,
            string appSetting,
            Func<Exception> invalidOperationException)
        {
            if (string.IsNullOrWhiteSpace(appSetting))
            {
                throw invalidOperationException();
            }

            var prop = (PropertyInfo) ((MemberExpression) propInfo.Body).Member;

            prop.SetValue(CalendarProviderConfig, appSetting, null);
        }

        public void ConfigureGrid(CalendarGridConfig calendarGridConfig)
        {
            CalendarGrid.Configure(calendarGridConfig);
        }

        public Task<bool> Populate()
        {
            Dictionary<string, string> userAliasMap = null;

            try
            {
                userAliasMap = CalendarManager.ResolveUserAccounts(UserDataResolver, CalendarProviderConfig.TeamList);

                if ((userAliasMap == null) || !userAliasMap.Any())
                {
                    throw new InvalidOperationException("no user account data found.");
                }
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(HtmlTools.GetHtmlError(ex.Message));
                return Task.FromResult(false);
            }

            try
            {
                var userAccounts = userAliasMap.Select(kvp => kvp.Key).ToList();
                var cInfoList =
                    CalendarManager.ResolveCalendarData(CalendarDataProvider, CalendarGridConfig.StartDate, userAccounts).ToList();

                CalendarGrid.Populate(cInfoList, userAliasMap.Keys.Select(strUser => new CalendarResource {Name = userAliasMap[strUser], Email = strUser}).ToList());
            }
            catch (Exception ex)
            {
                TraceTool.Instance.Value.TraceException(ex);
                LastErrors.Add(HtmlTools.GetHtmlError(ex.Message));
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> SetScreenResolution(int width, int height)
        {
            if (CalendarGridConfig == null)
            {
                return Task.FromResult(false);
            }

            ScreenSize = new ScreenSize
            {
                Width = width,
                Height = height
            };

            if (!ScreenSize.IsEmpty)
            {
                CalendarGridConfig.Width = ScreenSize.Width;
            }

            return Task.FromResult(true);
        }
    }
}