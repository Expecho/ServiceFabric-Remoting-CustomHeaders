using System;
using System.Reflection;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.Remoting.CustomHeaders.Util
{
    internal static class IdUtilHelper
    {
        private static readonly Func<MethodInfo, int> ComputeIdFromMethodDelegate;
        private static readonly Func<Type, int> ComputeIdFromTypeDelegate;
        private static readonly Func<string, string, int> ComputeIdFromNamesDelegate;

        static IdUtilHelper()
        {
            try
            {
                Type idUtilType = typeof(ServiceRuntime).Assembly.GetType("Microsoft.ServiceFabric.Services.Common.IdUtil");

                // The ComputeId methods are all internal. However, we specify both NonPublic and Public, just in cases
                // service fabric changed them to become public in the future, then hopefully this code would still work.
                MethodInfo computeIdFromMethod = idUtilType?.GetMethod(
                    "ComputeId",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new[] { typeof(MethodInfo) },
                    null);
                MethodInfo computeIdFromType = idUtilType?.GetMethod(
                    "ComputeId",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new[] { typeof(Type) },
                    null);
                MethodInfo computeIdFromNames = idUtilType?.GetMethod(
                    "ComputeId",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new[] { typeof(string), typeof(string) },
                    null);

                if (computeIdFromMethod != null)
                {
                    ComputeIdFromMethodDelegate = (Func<MethodInfo, int>)Delegate.CreateDelegate(typeof(Func<MethodInfo, int>), computeIdFromMethod);
                }

                if (computeIdFromType != null)
                {
                    ComputeIdFromTypeDelegate = (Func<Type, int>)Delegate.CreateDelegate(typeof(Func<Type, int>), computeIdFromType);
                }

                if (computeIdFromNames != null)
                {
                    ComputeIdFromNamesDelegate = (Func<string, string, int>)Delegate.CreateDelegate(typeof(Func<string, string, int>), computeIdFromNames);
                }
            }
            catch (Exception)
            {
                // Can't let the static constructor brings down the process if anything happens.
                // At worst, we just don't get any method namese.
            }
        }

        internal static int ComputeId(MethodInfo methodInfo)
        {
            return ComputeIdFromMethodDelegate?.Invoke(methodInfo) ?? 0;
        }

        internal static int ComputeId(Type type)
        {
            return ComputeIdFromTypeDelegate?.Invoke(type) ?? 0;
        }

        internal static int ComputeId(string typeName, string typeNamespace)
        {
            return ComputeIdFromNamesDelegate?.Invoke(typeName, typeNamespace) ?? 0;
        }
    }
}