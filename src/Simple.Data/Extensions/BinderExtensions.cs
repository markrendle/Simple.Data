namespace Simple.Data.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CSharp.RuntimeBinder;

    internal static class BinderExtensions
    {
        private static readonly Type TypeOfICSharpInvokeMemberBinder;
        private static readonly PropertyInfo ResultDiscardedProperty;
        private static readonly PropertyInfo TypeArgumentsProperty;
        private static readonly Func<InvokeMemberBinder, bool> ResultDiscardedGetter;
        private static readonly Func<InvokeMemberBinder, Type> TypeArgumentsGetter;

        static BinderExtensions()
        {
            // Microsoft are hiding the good stuff again. Not having that.
            try
            {
                TypeOfICSharpInvokeMemberBinder = Assembly.GetAssembly(typeof (CSharpArgumentInfo))
                    .GetType("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
                if (TypeOfICSharpInvokeMemberBinder != null)
                {
                    ResultDiscardedProperty = TypeOfICSharpInvokeMemberBinder.GetProperty("ResultDiscarded");
                    if (ResultDiscardedProperty != null)
                    {
                        ResultDiscardedGetter = GetResultDiscardedImpl;
                    }
                    TypeArgumentsProperty = TypeOfICSharpInvokeMemberBinder.GetProperty("TypeArguments");
                    if (TypeArgumentsProperty != null)
                    {
                        TypeArgumentsGetter = GetGenericParameterImpl;
                    }
                }
            }
            catch
            {
                ResultDiscardedGetter = null;
                TypeArgumentsGetter = null;
            }

            ResultDiscardedGetter = ResultDiscardedGetter ?? (_ => false);
            TypeArgumentsGetter = TypeArgumentsGetter ?? (_ => null);
        }

        public static bool HasSingleUnnamedArgument(this InvokeMemberBinder binder)
        {
            return binder.CallInfo.ArgumentCount == 1 &&
                   (binder.CallInfo.ArgumentNames.Count == 0 ||
                    string.IsNullOrWhiteSpace(binder.CallInfo.ArgumentNames[0]));
        }

        public static bool IsResultDiscarded(this InvokeMemberBinder binder)
        {
            return ResultDiscardedGetter(binder);
        }

        public static Type GetGenericParameter(this InvokeMemberBinder binder)
        {
            return TypeArgumentsGetter(binder);
        }

        private static bool GetResultDiscardedImpl(InvokeMemberBinder binder)
        {
            if (!TypeOfICSharpInvokeMemberBinder.IsInstanceOfType(binder)) return false;

            try
            {
                return (bool) ResultDiscardedProperty.GetValue(binder, null);
            }
            catch (ArgumentException)
            {
                return true;
            }
            catch (TargetException)
            {
                return true;
            }
            catch (TargetParameterCountException)
            {
                return true;
            }
            catch (MethodAccessException)
            {
                return true;
            }
            catch (TargetInvocationException)
            {
                return true;
            }
        }
        private static Type GetGenericParameterImpl(InvokeMemberBinder binder)
        {
            if (!TypeOfICSharpInvokeMemberBinder.IsInstanceOfType(binder)) return null;

            try
            {
                return ((IEnumerable<Type>)TypeArgumentsProperty.GetValue(binder, null)).FirstOrDefault();
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (TargetException)
            {
                return null;
            }
            catch (TargetParameterCountException)
            {
                return null;
            }
            catch (MethodAccessException)
            {
                return null;
            }
            catch (TargetInvocationException)
            {
                return null;
            }
        }
    }
}