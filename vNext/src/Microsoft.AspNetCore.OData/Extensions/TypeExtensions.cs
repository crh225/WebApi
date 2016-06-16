using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using System.Linq;
using Microsoft.AspNetCore.OData.Reflection;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class TypeExtensions
    {
        internal static MethodInfo GetMethodInternal(this Type type, string name, Type[] types)
        {
            var method = type.GetRuntimeMethod(name, types);
            return method;
        }
        internal static MethodInfo GetMethodInternal(this Type type, string name, BindingFlagsInternal flags)
        {
            var method = type.GetRuntimeMethods().SingleOrDefault(m => m.Name == name &&
                (type.GetTypeInfo().IsInterface || m.MatchesFlags(flags)));
            return method;
        }

        internal static MethodInfo[] GetMethodsInternal(this Type type, BindingFlagsInternal flags)
        {
            var methods = type.GetRuntimeMethods().Where(m =>
                type.GetTypeInfo().IsInterface || m.MatchesFlags(flags)).ToArray();
            return methods;
        }

        internal static PropertyInfo[] GetPropertiesInternal(this Type type, BindingFlagsInternal flags)
        {
            var properties = type.GetRuntimeProperties().Where(m =>
                type.GetTypeInfo().IsInterface || m.GetMethod.MatchesFlags(flags)).ToArray();
            return properties;
        }

        private static bool MatchesFlags(this MethodBase member, BindingFlagsInternal flags)
        {
            if ((flags & BindingFlagsInternal.Instance) == BindingFlagsInternal.Instance)
            {
                if (!member.IsStatic)
                {
                    return true;
                }
            }
            if ((flags & BindingFlagsInternal.NonPublic) == BindingFlagsInternal.NonPublic)
            {
                if (!member.IsPublic)
                {
                    return true;
                }
            }
            if ((flags & BindingFlagsInternal.Public) == BindingFlagsInternal.Public)
            {
                if (member.IsPublic)
                {
                    return true;
                }
            }
            if ((flags & BindingFlagsInternal.Static) == BindingFlagsInternal.Static)
            {
                if (member.IsStatic)
                {
                    return true;
                }
            }
            return false;
        }
        public static string EdmFullName(this Type clrType)
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrType.Namespace, clrType.Name);
        }

        public static bool IsNullable(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                // value types are only nullable if they are Nullable<T>
                return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }

        public static Type ToNullable(this Type t)
        {
            if (t.IsNullable())
            {
                return t;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(t);
            }
        }

        public static bool IsCollection(this Type type)
        {
            Type elementType;
            return type.IsCollection(out elementType);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            elementType = type;

            // see if this type should be ignored.
            if (type == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.GetTypeInfo().IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        internal static TypeCodeInternal GetTypeCode(this Type type)
        {
            if (type == null)
                return TypeCodeInternal.Empty;
            else if (type == typeof(bool))
                return TypeCodeInternal.Boolean;
            else if (type == typeof(char))
                return TypeCodeInternal.Char;
            else if (type == typeof(sbyte))
                return TypeCodeInternal.SByte;
            else if (type == typeof(byte))
                return TypeCodeInternal.Byte;
            else if (type == typeof(short))
                return TypeCodeInternal.Int16;
            else if (type == typeof(ushort))
                return TypeCodeInternal.UInt16;
            else if (type == typeof(int))
                return TypeCodeInternal.Int32;
            else if (type == typeof(uint))
                return TypeCodeInternal.UInt32;
            else if (type == typeof(long))
                return TypeCodeInternal.Int64;
            else if (type == typeof(ulong))
                return TypeCodeInternal.UInt64;
            else if (type == typeof(float))
                return TypeCodeInternal.Single;
            else if (type == typeof(double))
                return TypeCodeInternal.Double;
            else if (type == typeof(decimal))
                return TypeCodeInternal.Decimal;
            else if (type == typeof(System.DateTime))
                return TypeCodeInternal.DateTime;
            else if (type == typeof(string))
                return TypeCodeInternal.String;
            else if (type.GetTypeInfo().IsEnum)
                return GetTypeCode(Enum.GetUnderlyingType(type));
            else
                return TypeCodeInternal.Object;
        }
    }
}