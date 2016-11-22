var TeamCalendarScripts =
{
    IsLiveData: false,

    Setup: {
        ServiceUrl: null,
        ServerErrorsRef: null,
        ServerConfig: null,
        CalendarGridRef: null,
        MainTimerRef: null,
        HoverBoxRef: null,
        ClockBoxRef: null,
        LiveBoxRef: null,
        DateBoxRef: null
    },

    Service: {
        PostWebServiceData:
            function(methodName, methodParams, successCallback, errCallback, contentType, endpointType) {
                if (TeamCalendarScripts.Setup.ServiceUrl == null) {
                    console.error("ServiceUrl not defined.");
                    return;
                }

                if (contentType == null) {
                    contentType = "application/json";
                }

                if (endpointType == null) {
                    endpointType = "client";
                }

                var webMethod = TeamCalendarScripts.Setup.ServiceUrl + endpointType + "/" + methodName;

                $("#loader").show();

                $.ajax({
                        type: "POST",
                        async: true,
                        url: webMethod,
                        data: methodParams,
                        success: successCallback,
                        contentType: contentType,
                        dataType: "json"
                    })
                    .done(function() { $("#loader").hide(); })
                    .fail(function() { $("#loader").hide(); })
                    .fail(errCallback);
            },

        GetWebServiceData: function(methodName, methodParams, successCallback, errCallback, endpointType) {
            if (TeamCalendarScripts.Setup.ServiceUrl == null) {
                console.error("ServiceUrl not defined.");
                return;
            }

            if (endpointType == null) {
                endpointType = "client";
            }

            var webMethod = TeamCalendarScripts.Setup.ServiceUrl + endpointType + "/" + methodName;

            $("#loader").show();

            $.ajax(webMethod, { async: true })
                .done(function() { $("#loader").hide(); })
                .done(successCallback)
                .fail(function() { $("#loader").hide(); })
                .fail(errCallback);
        },

        Ping: function(postActionSuccess, postActionFailure) {
            TeamCalendarScripts.Service.GetWebServiceData("Ping",
                null,
                function(msg) {
                    TeamCalendarScripts.IsLiveData = true;
                    if (postActionSuccess) {
                        postActionSuccess();
                    }
                },
                function(e) {
                    TeamCalendarScripts.IsLiveData = false;
                    if (postActionFailure) {
                        postActionFailure();
                    }
                    console.error("Unable to reach server: invalid/unavailable service url?");
                }
            );
        },

        HandleServiceFailureAndRetry: function(e) {
            TeamCalendarScripts.IsLiveData = false;
            window.clearTimeout(TeamCalendarScripts.Setup.MainTimerRef);
            TeamCalendarScripts.Setup.MainTimerRef = window.setTimeout(TeamCalendarScripts.Initialize, 5000);
            console.error(e);
            TeamCalendarScripts.Service.GetServerErrors(function(result) {
                TeamCalendarScripts.Setup.ServerErrorsRef.html(result);
            });
        },

        GetServerErrors: function(postAction) {
            TeamCalendarScripts.Service.GetWebServiceData("GetServerErrors",
                null,
                function(result) {
                    console.error(e);
                    if (postAction) {
                        postAction(result);
                    }
                },
                function(e) {
                    console.error(e);
                    if (postAction) {
                        postAction(e);
                    }
                },
                "server"
            );
        },

        PullSpecificDayCalendar: function(dateShift) {
            if (!TeamCalendarScripts.Setup.CalendarGridRef) {
                console.warn();
                return;
            }

            var params = "=" + dateShift;
            TeamCalendarScripts.Service.PostWebServiceData("GetSpecificCalendarGridView",
                params,
                function(data) {
                    TeamCalendarScripts.Setup.CalendarGridRef.html(data);
                    TeamCalendarScripts.IsLiveData = false;
                    TeamCalendarScripts.UI.SetupDate();
                    TeamCalendarScripts.Setup.ServerConfig.HoverBoxEnabled = false;
                    TeamCalendarScripts.UI.SetupHoverBox();
                    window.clearTimeout(TeamCalendarScripts.Setup.MainTimerRef);
                    TeamCalendarScripts.Setup.MainTimerRef = window
                        .setTimeout(TeamCalendarScripts.Service.ScheduledRefresh, 60000);
                },
                function(e) {
                    TeamCalendarScripts.Service.HandleServiceFailureAndRetry(e);
                },
                "application/x-www-form-urlencoded",
                "client"
            );
        },

        PullCalendarView: function() {
            if (!TeamCalendarScripts.Setup.CalendarGridRef) {
                console.warn();
                return;
            }

            TeamCalendarScripts.Service.GetWebServiceData("GetCalendarGridView",
                null,
                function(data) {
                    TeamCalendarScripts.Setup.CalendarGridRef.html(data);
                    TeamCalendarScripts.IsLiveData = true;
                    TeamCalendarScripts.UI.SetupDate();
                    TeamCalendarScripts.UI.SetupHoverBox();
                    window.clearTimeout(TeamCalendarScripts.Setup.MainTimerRef);
                    TeamCalendarScripts.Setup.MainTimerRef = window
                        .setTimeout(TeamCalendarScripts.Service.ScheduledRefresh, 30000);
                },
                function(e) {
                    TeamCalendarScripts.Service.HandleServiceFailureAndRetry(e);
                });
        },

        ScheduledRefresh: function() {
            TeamCalendarScripts.Service.Ping(TeamCalendarScripts.Service.PullCalendarView,
                function() {
                    TeamCalendarScripts.UI.SetupHoverBox();
                    window.setTimeout(TeamCalendarScripts.Service.ScheduledRefresh, 30000);
                });
        }
    },

    UI: {
        ClockData: function(hour, minute) {
            TeamCalendarScripts.Hour = hour;
            TeamCalendarScripts.Minute = minute;
        },

        GetClockInfo: function() {
            var dt = new Date();
            var hour = dt.getHours();
            var min = dt.getMinutes();
            if (min <= 9) {
                min = "0" + min;
            }
            if (hour <= 9) {
                hour = "0" + hour;
            }

            return new TeamCalendarScripts.UI.ClockData(hour, min);
        },

        GetWeekNumber: function(headerRef, postAction) {
            TeamCalendarScripts.Service.GetWebServiceData("GetWeekNo",
                null,
                function(result) {
                    headerRef.html(function(index, text) {
                        return text.replace(/[{].*[}]/, result);
                    });
                    headerRef.show();

                    if (postAction) {
                        postAction();
                    }
                },
                function(e) {
                    console.error(e);
                    headerRef.html("<!--no weekno--!>");
                    window.setTimeout(TeamCalendarScripts.Service.ScheduledRefresh, 10000);
                }
            );
        },

        GetLocalizedDate: function(headerRef, postAction) {
            TeamCalendarScripts.Service.GetWebServiceData("GetLocalizedDate",
                null,
                function(result) {
                    headerRef.html(result);
                    headerRef.show();

                    if (postAction) {
                        postAction();
                    }
                },
                function(e) {
                    console.error(e);
                    headerRef.html("<!--no datetime--!>");
                    window.setTimeout(TeamCalendarScripts.Service.ScheduledRefresh, 10000);
                }
            );
        },

        SetScreenResolution: function(objRef) {
            var myWidth = $(objRef).width();
            var myHeight = $(objRef).height();

            var params = JSON.stringify({ "width": myWidth, "height": myHeight });

            TeamCalendarScripts.Service.PostWebServiceData("SetScreenResolution",
                params,
                function(msg) {
                },
                function(e) {}
            );
        },

        SetDateShift: function(dateShift, apiKey) {
            var params = JSON.stringify({ "dateShift": dateShift, "apiKey": apiKey });

            TeamCalendarScripts.Service.PostWebServiceData("SetDate",
                params,
                function(msg) {
                    TeamCalendarScripts.Service.PullCalendarView();
                },
                function(e) {
                    console.error(e);
                },
                null,
                "server"
            );
        },

        ForceFullScreen: function() {
            try {
                if (!document.fullscreenElement &&
                    !document.mozFullScreenElement &&
                    !document.webkitFullscreenElement &&
                    !document.msFullscreenElement) {
                    if (document.documentElement.requestFullscreen) {
                        document.documentElement.requestFullscreen();
                    } else if (document.documentElement.msRequestFullscreen) {
                        document.documentElement.msRequestFullscreen();
                    } else if (document.documentElement.mozRequestFullScreen) {
                        document.documentElement.mozRequestFullScreen();
                    } else if (document.documentElement.webkitRequestFullscreen) {
                        document.documentElement.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
                    }
                } else {
                    if (document.exitFullscreen) {
                        document.exitFullscreen();
                    } else if (document.msExitFullscreen) {
                        document.msExitFullscreen();
                    } else if (document.mozCancelFullScreen) {
                        document.mozCancelFullScreen();
                    } else if (document.webkitExitFullscreen) {
                        document.webkitExitFullscreen();
                    }
                }
            } catch (e) {
                console.warn(e);
            }
        },

        UserListRefresh: function() {
            TeamCalendarScripts.GetWebServiceData("RefreshUserList", null, null, null);
        },

        SetupHoverBox: function() {
            if (!TeamCalendarScripts.Setup.HoverBoxRef) {
                console.warn();
                return;
            }

            var hoverboxRef = TeamCalendarScripts.Setup.HoverBoxRef;

            if (!TeamCalendarScripts.Setup.ServerConfig || !TeamCalendarScripts.Setup.ServerConfig.HoverBoxEnabled) {
                if (hoverboxRef) {
                    hoverboxRef.hide();
                }
                return;
            }

            if (new Date().getHours() < TeamCalendarScripts.Setup.ServerConfig.BusinessBeginsHour) return;
            if (new Date().getHours() > TeamCalendarScripts.Setup.ServerConfig.BusinessEndsHour) return;

            var refId = TeamCalendarScripts.Setup.CalendarGridRef;
            var ctrlOffset = refId.offset();

            var width = (new Date().getHours() - TeamCalendarScripts.Setup.ServerConfig.BusinessBeginsHour) * TeamCalendarScripts.Setup.ServerConfig.CellWidth;
            var widthAdj = new Date().getMinutes() * (TeamCalendarScripts.Setup.ServerConfig.CellWidth / TeamCalendarScripts.Setup.ServerConfig.CellDuration);

            hoverboxRef.css("left", TeamCalendarScripts.Setup.ServerConfig.RowHeaderWidth + ctrlOffset.left + "px");
            hoverboxRef.css("top", TeamCalendarScripts.Setup.ServerConfig.HeaderHeight + ctrlOffset.top + "px");
            hoverboxRef.css("width", width + widthAdj + "px");
            hoverboxRef.css("height", refId.height() - TeamCalendarScripts.Setup.ServerConfig.HeaderHeight + "px");
            hoverboxRef.show();
        },

        SetupClock: function() {
            if (!TeamCalendarScripts.Setup.ClockBoxRef) {
                return;
            }

            var ctrlOffset = TeamCalendarScripts.Setup.CalendarGridRef.offset();

            var clockBoxRef = TeamCalendarScripts.Setup.ClockBoxRef;
            clockBoxRef.html("..:..");
            var width = TeamCalendarScripts.Setup.ServerConfig.RowHeaderWidth +
                12 * TeamCalendarScripts.Setup.ServerConfig.CellWidth +
                ctrlOffset.left - clockBoxRef.width() - 30;
            clockBoxRef.css("top", ctrlOffset.top - 50 + "px");
            clockBoxRef.css("left", width + "px");
            clockBoxRef.show();

            TeamCalendarScripts.UI.UpdateClock();
        },

        UpdateClock: function() {
            if (!TeamCalendarScripts.Setup.ClockBoxRef) {
                return;
            }

            var clockBoxRef = TeamCalendarScripts.Setup.ClockBoxRef;

            clockBoxRef.html(TeamCalendarScripts.UI.GetClockInfo().toString());

            window.setTimeout(TeamCalendarScripts.UI.UpdateClock, 1000);
        },

        SetupDate: function() {
            if (!TeamCalendarScripts.Setup.CalendarGridRef) {
                return;
            }

            var ctrlOffset = TeamCalendarScripts.Setup.CalendarGridRef.offset();

            var dateHeader = TeamCalendarScripts.Setup.DateBoxRef;
            if (dateHeader) {
                TeamCalendarScripts.UI.GetLocalizedDate(dateHeader,
                    function() {
                        dateHeader.css("top", ctrlOffset.top - 40 + "px");
                        TeamCalendarScripts.UI.SetupWeekNo(dateHeader);
                    });
            }
        },

        SetupLiveLabel: function() {
            if (!TeamCalendarScripts.Setup.ClockBoxRef) {
                return;
            }

            var ctrlOffset = TeamCalendarScripts.Setup.ClockBoxRef.offset();

            var liveBoxRef = TeamCalendarScripts.Setup.LiveBoxRef;
            if (liveBoxRef) {
                liveBoxRef.css("left", ctrlOffset.left + 21 + "px");
                liveBoxRef.css("top", ctrlOffset.top + 30 + "px");
                liveBoxRef.show();
            }

            window.setInterval(TeamCalendarScripts.UI.LiveLabelBlink, 1000);
        },

        LiveLabelBlink: function() {
            if (!TeamCalendarScripts.Setup.ClockBoxRef) {
                return;
            }

            var clockBoxRef = TeamCalendarScripts.Setup.ClockBoxRef;

            var liveBoxRef = TeamCalendarScripts.Setup.LiveBoxRef;
            if (!liveBoxRef) return;

            if (!TeamCalendarScripts.IsLiveData) {
                liveBoxRef.css("left", clockBoxRef.offset().left + "px");
                liveBoxRef.removeClass("livebox_livemode");
                liveBoxRef.addClass("livebox_offlinemode");
                liveBoxRef.html("OFFLINE: " + TeamCalendarScripts.UI.GetClockInfo().toString());
                return;
            } else {
                liveBoxRef.css("left", clockBoxRef.offset().left + 21 + "px");
                liveBoxRef.removeClass("livebox_offlinemode");
                liveBoxRef.toggleClass("livebox_livemode");
                liveBoxRef.html("[LIVE]");
            }
        }
    },

    Initialize: function() {
        if (TeamCalendarScripts.Setup.ServiceUrl == null) {
            console.error("service url is not defined.");
            return;
        }

        if (TeamCalendarScripts.Setup == null) {
            console.error("clientConfig is not defined.");
            return;
        }

        TeamCalendarScripts.UI.ForceFullScreen();

        this.Service.Ping(
            function() {
                TeamCalendarScripts.UI.SetScreenResolution(TeamCalendarScripts.Setup.MainFrameRef);
                TeamCalendarScripts.Service.GetWebServiceData("GetConfig",
                    null,
                    function(cfg) {
                        if (cfg == null) {
                            TeamCalendarScripts.IsLiveData = false;
                            return;
                        }

                        TeamCalendarScripts.Setup.ServerConfig = cfg;
                        TeamCalendarScripts.UI.SetupClock();
                        TeamCalendarScripts.UI.SetupLiveLabel();
                        TeamCalendarScripts.Service.PullCalendarView();
                        TeamCalendarScripts.UI.SetupDateShiftUI();
                    },
                    function(e) {
                        TeamCalendarScripts.Service.HandleServiceFailureAndRetry(e);
                    },
                    "server");
            },
            function(e) {
                TeamCalendarScripts.Service.HandleServiceFailureAndRetry(e);
            });
    }
};

TeamCalendarScripts.UI.ClockData.prototype.toString = function() {
    return TeamCalendarScripts.Hour + ":" + TeamCalendarScripts.Minute;
};

TeamCalendarScripts.UI.SetupWeekNo = function(dateHeader) {
    var weekNoHeader = $("#WeekNumberBox");
    if (weekNoHeader) {
        TeamCalendarScripts.UI.GetWeekNumber(weekNoHeader,
            function() {
                weekNoHeader.css("top", dateHeader.offset().top + "px");
                weekNoHeader.css("left", dateHeader.offset().left + dateHeader.width() + 50 + "px");
            });
    }
};

TeamCalendarScripts.UI.SetupDateShiftUI = function() {
    var timeshiftHeader = $("#timeshift");

    if (!timeshiftHeader) {
        return;
    }

    if (!TeamCalendarScripts.Setup.ServerConfig || !TeamCalendarScripts.Setup.ServerConfig.DateShiftEnabled) {
        timeshiftHeader.hide();
        return;
    }

    timeshiftHeader.show();

    $("#timeshift_now").hide();

    $("#timeshift_prev")
        .bind({
            click: function() {
                $(this).parent().find("a").removeClass("activelink");

                var localCnt = $(this).parent().data("dateshift");
                if (localCnt == null) {
                    localCnt = 0;
                }
                localCnt--;
                TeamCalendarScripts.Service.PullSpecificDayCalendar(localCnt);

                if (TeamCalendarScripts.Setup.ServerConfig.MaxDaysShift != null &&
                    TeamCalendarScripts.Setup.ServerConfig.MaxDaysShift < Math.abs(localCnt) + 1) {
                    $(this).hide();
                } else {
                    $(this).parent().data("dateshift", localCnt);
                    $(this).addClass("activelink");
                }

                $("#timeshift_now").show();
            }
        });

    $("#timeshift_next")
        .bind({
            click: function() {
                $(this).parent().find("a").removeClass("activelink");

                var localCnt = $(this).parent().data("dateshift");
                if (localCnt == null) {
                    localCnt = 0;
                }
                localCnt++;
                TeamCalendarScripts.Service.PullSpecificDayCalendar(localCnt);

                if (TeamCalendarScripts.Setup.ServerConfig.MaxDaysShift != null &&
                    TeamCalendarScripts.Setup.ServerConfig.MaxDaysShift < Math.abs(localCnt) + 1) {
                    $(this).hide();
                } else {
                    $(this).parent().data("dateshift", localCnt);
                    $(this).addClass("activelink");
                }

                $("#timeshift_now").show();
            }
        });

    $("#timeshift_now")
        .bind({
            click: function() {
                $(this).parent().data("dateshift", 0);
                TeamCalendarScripts.Setup.ServerConfig.HoverBoxEnabled = true;
                TeamCalendarScripts.Service.PullCalendarView();
                $(this).parent().find("a").removeClass("activelink");
                $("#timeshift_now").hide();
                $("#timeshift_prev").show();
                $("#timeshift_next").show();
            }
        });
};