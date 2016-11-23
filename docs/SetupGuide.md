# Team Calendar Setup Guide

## Server application configuration 

By default the application is configured to use demo / test data provider for calendar data. 
This and other behavior is configured via application configuration file (web.config)

### General configuration

**TeamCalendar.DiagnisticMode**
> Enables or disables diagnostics logging (server log file).
> Default: disabled

**TeamCalendar.TeamList**
> Specifies list of semicolon (”;”) separated people / resources
> Can be: AD user name, full name or email.

**TeamCalendar.RoomKeywords**
> Specifies keywords distinguishing room resources used for grouping.

**TeamCalendar.UICultureName**
> Specifies .Net UI culture
> Default: "en-GB"

**TeamCalendar.TimeZoneNumber**
> Specifies  client Time Zone offset.
> Default: 1

**TeamCalendar.DateshiftEnabled**
> Allows client switching between dates (next / prev)
> Default: false

**TeamCalendar.MaxDaysShift**
> Specifies maximum allowed date range
> Default: 2 

### Provider configuration

Data providers are pluggable, and the application is loading the configured one from application bin folder.

**TeamCalendar.CalendarDataProvider**
> Specifies data provider for the calendar. 
> MS Exchange calendar data provider: TeamCalendar.CalendarDataProvider.Exchange2010SP2
> Default: TeamCalendar.CalendarDataProvider.Test, which is offline, read-only stub for demonstration / frontend test purposes.

**TeamCalendar.CalendarUserDataResolver**
> Specifies data provider for resolving user names to emails and vice versa.
> Can use MS Exchange or Active Directory (AD) to lookup the required user information.
> MS Exchange provider name: TeamCalendar.CalendarDataProvider.Exchange2010SP2
> AD provider name: TeamCalendar.Common. AdCommon
> Default: TeamCalendar.CalendarDataProvider.Exchange2010SP2

**TeamCalendar.ExchangeUrl**
> Specifies web service url of calendar data provider. E.g. MS Exchange web service url.
> Example: https://[your_host_url]/ews/exchange.asmx

**TeamCalendar.AccountUser**
> Specifies user name for accessing calendar data provider.
> Required permissions: read-only access to view shared calendars.

**TeamCalendar.AccountPassword**
> Specifies password for accessing calendar data provider.

**TeamCalendar.AccountDomain**
> Specifies domain name for accessing calendar data provider.
