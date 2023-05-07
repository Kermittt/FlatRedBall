using System.Reflection;
using System;

namespace DynamicPluginPlugin.Extensions
{
    public static class TypeExtensions
    {
        public static string UnversionedName(this Type type)
        {
            var typeName = type.FullName;
            var assemblyName = type.Assembly.GetName().Name;

            return Assembly.CreateQualifiedName(assemblyName, typeName);
        }
    }
}
