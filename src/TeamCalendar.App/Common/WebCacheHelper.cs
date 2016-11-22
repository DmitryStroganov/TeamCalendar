using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.Caching;

namespace TeamCalendar.Common
{
    /// <summary>
    ///     Web caching helper methods.
    ///     Automatically creates cache item reference from calling method as
    ///     "$Namespace-ModuleVersionId$ClassName.MethodName$T".
    ///     Note: since its tied to MethodName it cannot be used across diferent method. Use custom cache key parameter for such scenarios.
    /// </summary>
    public static class WebCacheHelper
    {
        private const int m_cacheTimeout = 1200;

        private static readonly string m_cacheKeyMain = string.Format(CultureInfo.InvariantCulture,
            "{0}:{1}:{2}",
            (object) Assembly.GetExecutingAssembly().GetName().Name,
            (object) Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString(),
            (object) HttpRuntime.AppDomainAppId);

        private static readonly bool m_UseWebCaching = HttpRuntime.Cache != null;

        /// <summary>
        ///     Gets an object of Type T from cache
        /// </summary>
        public static T Get<T>()
        {
            return _Get<T>(null);
        }

        /// <summary>
        ///     Gets an object of Type T from cache
        /// </summary>
        /// <param name="key">additional reference key (for instance: content dependancy)</param>
        public static T Get<T>(string key)
        {
            return _Get<T>(key);
        }

        /// <summary>
        ///     Puts an object of Type T in cache.
        ///     Default timeout equals 1200 sec (30 min)
        /// </summary>
        public static void Set<T>(T data)
        {
            _Set(data, 1200.0, null);
        }

        /// <summary>
        ///     Puts an object of Type T in cache.
        /// </summary>
        /// <param name="cacheTimeOut">Cache Timeout, in seconds</param>
        public static void Set<T>(T data, double cacheTimeout)
        {
            _Set(data, cacheTimeout, null);
        }

        /// <summary>
        ///     Puts an object of Type T in cache.
        /// </summary>
        /// <param name="key">additional reference key (for instance: content dependancy)</param>
        public static void Set<T>(T data, string key)
        {
            _Set(data, 1200.0, key);
        }

        /// <summary>
        ///     Puts an object of Type T in cache.
        /// </summary>
        /// <param name="key">additional reference key (for instance: content dependancy)</param>
        /// <param name="cacheTimeOut">Cache Timeout, in seconds</param>
        public static void Set<T>(T data, string key, double cacheTimeout)
        {
            _Set(data, cacheTimeout, key);
        }

        private static T _Get<T>(string key)
        {
            if (!m_UseWebCaching)
            {
                throw new ArgumentNullException("key", "HttpContext not initialized.");
            }
            return (T) HttpRuntime.Cache[GetSourceCallHashCode<T>(key)];
        }

        private static void _Set<T>(T data, double cacheTimeOut, string key)
        {
            if (!m_UseWebCaching)
            {
                throw new ArgumentNullException("data", "HttpContext not initialized.");
            }
            var sourceCallHashCode = GetSourceCallHashCode<T>(key);
            if (data == null)
            {
                HttpRuntime.Cache.Remove(sourceCallHashCode);
            }
            else
            {
                HttpRuntime.Cache.Insert(sourceCallHashCode, data, null, DateTime.UtcNow.AddSeconds(cacheTimeOut), Cache.NoSlidingExpiration);
            }
        }

        private static string GetSourceCallHashCode<T>(string key)
        {
            string str = null;
            if (string.IsNullOrEmpty(key))
            {
                var stackTrace = new StackTrace();
                if (stackTrace.FrameCount > 1)
                {
                    var method = stackTrace.GetFrame(stackTrace.FrameCount > 2 ? 3 : 2).GetMethod();
                    var name1 = method.Name;
                    var name2 = method.ReflectedType.Name;
                    str = string.Format(CultureInfo.InvariantCulture,
                        "${0}${1}.{2}${3}",
                        (object) m_cacheKeyMain,
                        (object) name2,
                        (object) name1,
                        (object) typeof(T).Name);
                }
            }
            else
            {
                str = string.Format(CultureInfo.InvariantCulture, "${0}${1}${2}", (object) m_cacheKeyMain, (object) typeof(T).Name, (object) key);
            }
            if (string.IsNullOrEmpty(str))
            {
                str = string.Format(CultureInfo.InvariantCulture, "${0}${1}", (object) m_cacheKeyMain, (object) typeof(T).Name);
            }
            return str.Replace('`', '|');
        }
    }
}