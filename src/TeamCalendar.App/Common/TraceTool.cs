using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

namespace TeamCalendar.Common
{
    [SecurityCritical]
    public class TraceTool
    {
        public static readonly Lazy<Func<TraceToolSettings, TraceTool>> InstanceInitializer = new Lazy<Func<TraceToolSettings, TraceTool>>(() => cfg => new TraceTool(cfg));

        public static readonly Lazy<TraceTool> Instance = new Lazy<TraceTool>(() => Settings != null ? InstanceInitializer.Value(Settings) : null);

        private TraceTool(TraceToolSettings settings)
        {
            Settings = settings;

            if (!settings.Enabled)
            {
                return;
            }

            var cacheKey = $"{AppDomain.CurrentDomain.FriendlyName}.{AppDomain.CurrentDomain.Id}";

            if (settings.TraceUnhandledExceptions)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            }

            string currentAppDir;
            if (IsWebContextAvailable)
            {
                if (string.IsNullOrEmpty(settings.VirtualPath))
                {
                    currentAppDir = Path.Combine(HttpContext.Current.Server.MapPath("/"), "_logs");
                }
                else
                {
                    currentAppDir = Path.Combine(HttpContext.Current.Server.MapPath(settings.VirtualPath), "_logs");
                }
            }
            else
            {
                currentAppDir = Path.Combine(Environment.CurrentDirectory, "_logs");
            }

            if (!string.IsNullOrEmpty(settings.AltLogsPath))
            {
                currentAppDir = settings.AltLogsPath;
            }

            TextWriterTraceListener traceWritter = null;
            if (IsWebContextAvailable)
            {
                traceWritter = HttpContext.Current.Application[cacheKey] as TextWriterTraceListener;
            }

            if (traceWritter == null)
            {
                if (!settings.RedirectToConsole)
                {
                    if (string.IsNullOrEmpty(currentAppDir))
                    {
                        throw new ArgumentNullException("VirtualPath or AltLogsPath", "Log path cannot be null.");
                    }

                    try
                    {
                        if (!Directory.Exists(currentAppDir))
                        {
                            Directory.CreateDirectory(currentAppDir);
                        }
                    }
                    catch
                    {
                        throw;
                    }

                    var logFile = string.Format(CultureInfo.InvariantCulture,
                        @"{0}\{1}.log",
                        currentAppDir,
                        DateTime.UtcNow.ToString("ddMMyyyy", CultureInfo.InvariantCulture));

                    if (traceWritter == null)
                    {
                        traceWritter = new TextWriterTraceListener(logFile);
                    }
                }
                else
                {
                    if (traceWritter == null)
                    {
                        traceWritter = new TextWriterTraceListener(Console.Out);
                    }
                }
            }

            Trace.AutoFlush = true;

            if (!Trace.Listeners.Contains(traceWritter))
            {
                Trace.Listeners.Add(traceWritter);
            }

            if (IsWebContextAvailable)
            {
                HttpContext.Current.Application[cacheKey] = traceWritter;
            }
        }

        private static bool IsWebContextAvailable
        {
            get { return HttpContext.Current != null; }
        }

        private static TraceToolSettings Settings { get; set; }


        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "In this context, exception details should be bypassed to tracer only.")]
        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exceptionSummary = "!UnhandledException!";
            TraceException(new Exception(exceptionSummary, (Exception) e.ExceptionObject), 2);
        }

        /// <summary>
        ///     For diagnostic messages
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void Diag(string msg)
        {
            if (Settings.TraceDiag)
            {
                _TraceMessage(msg, GetDiagnosticsCallerStackFrame());
            }
        }

        /// <summary>
        ///     For diagnostic messages with formatting
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void Diag(string msg, params object[] args)
        {
            if (Settings.TraceDiag)
            {
                _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), GetDiagnosticsCallerStackFrame());
            }
        }

        /// <summary>
        ///     For diagnostic messages with custom formatting
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void DiagCustomFormat(string msg, params object[] args)
        {
            if (Settings.TraceDiag)
            {
                DiagCustomFormat(false, msg, args);
            }
        }

        /// <summary>
        ///     For diagnostic messages with custom formatting
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void DiagCustomFormat(bool includeNewline, string msg, params object[] args)
        {
            if (Settings.TraceDiag)
            {
                if (includeNewline)
                {
                    Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, msg, args));
                }
                else
                {
                    Trace.Write(string.Format(CultureInfo.InvariantCulture, msg, args));
                }
                Trace.Flush();
            }
        }

        internal void _Diag(short frameOffset, string msg, params object[] args)
        {
            if (Settings.TraceDiag)
            {
                _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), frameOffset);
            }
        }

        /// <summary>
        ///     For warning messages
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void Warn(string msg)
        {
            if (Settings.TraceWarnings)
            {
                _TraceMessage(msg, 2);
            }
        }

        /// <summary>
        ///     For warning messages with formatting
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void WarnFormat(string msg, object args)
        {
            if (Settings.TraceWarnings)
            {
                _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), 2);
            }
        }

        /// <summary>
        ///     For warning messages with formatting
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void WarnFormat(string msg, params object[] args)
        {
            if (Settings.TraceWarnings)
            {
                _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), 2);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Implicit shortnaming.")]
        public void WarnFormat(short frameOffset, string msg, params object[] args)
        {
            if (Settings.TraceWarnings)
            {
                _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), frameOffset);
            }
        }

        /// <summary>
        ///     For general purpose tracing
        /// </summary>
        public void TraceMessage(string msg)
        {
            _TraceMessage(msg, 2);
        }

        /// <summary>
        ///     For general purpose tracing with formatting
        /// </summary>
        public void TraceMessageFormat(string msg, params object[] args)
        {
            _TraceMessage(string.Format(CultureInfo.InvariantCulture, msg, args), 2);
        }

        /// <summary>
        ///     General purpose exception tracing
        /// </summary>
        public void TraceException(Exception ex)
        {
            TraceException(ex, 2);
        }

        /// <summary>
        ///     General purpose exception tracing
        /// </summary>
        public void TraceException(Exception ex, short frameOffset)
        {
            if (frameOffset < 1)
            {
                frameOffset = 0;
            }

            var dateTimeFormatted = string.IsNullOrEmpty(Settings.DateTimeFormat)
                ? DateTime.UtcNow.ToString("g", CultureInfo.InvariantCulture)
                : DateTime.UtcNow.ToString(Settings.DateTimeFormat, CultureInfo.InvariantCulture);
            string ClassName = null;
            string MethodName = null;
            string _srcLoc = null;

            if (frameOffset > 0)
            {
                var stackTrace = new StackTrace();
                if (stackTrace.FrameCount > frameOffset)
                {
                    try
                    {
                        var stackFrame = stackTrace.GetFrame(frameOffset);
                        var _fName = stackFrame.GetFileName();
                        var _lNumber = stackFrame.GetFileLineNumber();
                        var _cNumber = stackFrame.GetFileColumnNumber();
                        _srcLoc = !string.IsNullOrEmpty(_fName)
                            ? string.Format(CultureInfo.InvariantCulture, "'{0}', position [{1},{2}]", _fName, _lNumber, _cNumber)
                            : null;
                        var methodBase = stackFrame.GetMethod();
                        MethodName = methodBase.Name;
                        ClassName = methodBase.ReflectedType.Name;
                    }
                    finally
                    {
                    }
                }
            }

            if (string.IsNullOrEmpty(ClassName))
            {
                ClassName = "_UnknownClass";
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                MethodName = "_UnknownMethod";
            }

            var exTxt = new StringBuilder();
            exTxt.AppendFormat("\n[{0}] at '{1}.{2}'", dateTimeFormatted, ClassName, MethodName);
            if (!string.IsNullOrEmpty(_srcLoc))
            {
                exTxt.AppendLine(_srcLoc);
            }

            exTxt.AppendLine();
            exTxt.AppendLine("<--");
            exTxt.Append(GetExceptionInfo(ex));
            exTxt.AppendLine();
            exTxt.AppendLine("-->");

            Trace.WriteLine(exTxt.ToString());
            Trace.Flush();
        }

        /// <summary>
        ///     General purpose tracing with optional frame offset
        /// </summary>
        public void TraceMessage(string msg, short frameOffset)
        {
            if (frameOffset < 1)
            {
                frameOffset = 0;
            }
            _TraceMessage(msg, frameOffset);
        }

        public void Flush()
        {
            Trace.Flush();
        }

        public class TraceToolSettings
        {
            public bool Enabled { get; set; }
            public bool TraceUnhandledExceptions { get; set; }
            public bool RedirectToConsole { get; set; }
            public bool TraceDiag { get; set; }
            public bool TraceWarnings { get; set; }
            public string VirtualPath { get; set; }
            public string AltLogsPath { get; set; }
            public string DateTimeFormat { get; set; }
        }

        #region private helpers

        private StackFrame GetDiagnosticsCallerStackFrame()
        {
            var fl = new StackTrace(true).GetFrames().ToArray();
            var r = fl.FirstOrDefault(frame => !frame.GetMethod().Name.ToLowerInvariant().Contains("diag"));
            var idx = Array.IndexOf(fl, r);
            return fl[idx];
        }

        private void _TraceMessage(string msg, StackFrame stackFrame)
        {
            var dateTimeFormatted = string.IsNullOrEmpty(Settings.DateTimeFormat)
                ? DateTime.UtcNow.ToString("g", CultureInfo.InvariantCulture)
                : DateTime.UtcNow.ToString(Settings.DateTimeFormat, CultureInfo.InvariantCulture);
            string className = null;
            string methodName = null;

            var methodBase = stackFrame.GetMethod();
            methodName = methodBase.Name;
            className = methodBase.ReflectedType.Name;

            if (string.IsNullOrEmpty(className))
            {
                className = "_UnknownClass";
            }

            if (string.IsNullOrEmpty(methodName))
            {
                methodName = "_UnknownMethod";
            }

            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}]", dateTimeFormatted));
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "at '{0}.{1}': {3}{2}", className, methodName, msg, Environment.NewLine));
            Trace.Flush();
        }

        private void _TraceMessage(string msg, short frameOffset)
        {
            var dateTimeFormatted = string.IsNullOrEmpty(Settings.DateTimeFormat)
                ? DateTime.UtcNow.ToString("g", CultureInfo.InvariantCulture)
                : DateTime.UtcNow.ToString(Settings.DateTimeFormat, CultureInfo.InvariantCulture);
            string ClassName = null;
            string MethodName = null;

            if (frameOffset > 0)
            {
                var stackTrace = new StackTrace();
                if (stackTrace.FrameCount >= frameOffset)
                {
                    var stackFrame = stackTrace.GetFrame(frameOffset);
                    var methodBase = stackFrame.GetMethod();
                    MethodName = methodBase.Name;
                    ClassName = methodBase.ReflectedType.Name;
                }
            }

            if (string.IsNullOrEmpty(ClassName))
            {
                ClassName = "_UnknownClass";
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                MethodName = "_UnknownMethod";
            }

            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}]", dateTimeFormatted));
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "at '{0}.{1}': {3}{2}", ClassName, MethodName, msg, Environment.NewLine));
            Trace.Flush();
        }

        private string GetExceptionInfo(Exception ex)
        {
            var aggregateEx = ex as AggregateException;
            if ((aggregateEx != null) && (aggregateEx.InnerExceptions.Count > 0))
            {
                ex = aggregateEx.InnerExceptions[0];
            }

            var _exceptionStack = new StringBuilder();

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                _exceptionStack.AppendFormat("{0}\n{1}\n", ex.Message, ex.StackTrace);
            }
            else
            {
                _exceptionStack.AppendFormat("{0}\n", ex.Message);
            }

            if (ex.InnerException != null)
            {
                m_StackTrace = new StringBuilder();
                _RetriveStackTraceRecursively(ex.InnerException);
                _exceptionStack.Append(m_StackTrace);
            }

            return _exceptionStack.ToString();
        }

        private static StringBuilder m_StackTrace = new StringBuilder();
        private static readonly short _rlim = 4;
        private static short _cnt;

        private void _RetriveStackTraceRecursively(Exception innerEx)
        {
            if (!string.IsNullOrEmpty(innerEx.StackTrace))
            {
                m_StackTrace.AppendFormat("{0}\n", innerEx.StackTrace);
            }
            if (_cnt > _rlim)
            {
                return;
            }
            _cnt++;
            if (innerEx.InnerException != null)
            {
                _RetriveStackTraceRecursively(innerEx.InnerException);
            }
        }

        #endregion
    }
}