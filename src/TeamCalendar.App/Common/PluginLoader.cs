using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;

namespace TeamCalendar.Common
{
    [SecurityCritical]
    public static class PluginLoader
    {
        public static T LoadSingleFrom<T>(string pluginPath, params object[] constructorParameters)
        {
            if (!pluginPath.EndsWith(".dll"))
            {
                pluginPath += ".dll";
            }

            if (!File.Exists(pluginPath))
            {
                throw new InvalidOperationException("Specified plugin not found: " + pluginPath);
            }

            var newAssembly = Assembly.LoadFrom(pluginPath);
            var pluginType = newAssembly.GetTypes().FirstOrDefault(type => type.IsClass && typeof(T).IsAssignableFrom(type));

            return CreateType<T>(pluginType, typeof(T).FullName, constructorParameters);
        }

        public static T LoadSpecificFrom<T>(string typeString, string pathRef)
        {
            if (typeString.Contains(","))
            {
                var p = typeString.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                return LoadSpecificSingleFrom<T>(p[0], pathRef + p[1]);
            }
            return LoadSingleFrom<T>(typeString);
        }

        private static T LoadSpecificSingleFrom<T>(string typeName, string pluginPath, params object[] constructorParameters)
        {
            if (!pluginPath.EndsWith(".dll"))
            {
                pluginPath += ".dll";
            }

            if (!File.Exists(pluginPath))
            {
                throw new InvalidOperationException("Specified plugin not found: " + pluginPath);
            }

            var pluginType = Assembly.LoadFrom(pluginPath).GetTypes().FirstOrDefault(type =>
                type.IsClass && typeof(T).IsAssignableFrom(type) &&
                string.Equals(type.FullName, typeName, StringComparison.InvariantCultureIgnoreCase));

            return CreateType<T>(pluginType, typeName, constructorParameters);
        }

        private static T CreateType<T>(Type pluginType, string typeName, object[] constructorParameters)
        {
            if (pluginType == null)
            {
                return default(T);
            }

            try
            {
                return (T) Activator.CreateInstance(pluginType, constructorParameters);
            }
            catch (Exception ex)
            {
                if (ex is TypeLoadException || ex is MissingMethodException)
                {
                    throw new InvalidOperationException($"Unable to load implementation for {typeof(T)} - '{typeName}' not found.", ex.InnerException);
                }

                throw;
            }
        }
    }
}