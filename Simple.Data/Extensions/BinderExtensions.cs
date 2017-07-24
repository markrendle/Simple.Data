namespace Shitty.Data.Extensions
{
    using System;
    using System.Dynamic;
    using System.Reflection;
    using Microsoft.CSharp.RuntimeBinder;

    internal static class BinderExtensions
    {
        private static readonly Type TypeOfICSharpInvokeMemberBinder;
        private static readonly PropertyInfo ResultDiscardedProperty;
        private static readonly Func<InvokeMemberBinder, bool> ResultDiscardedGetter;

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
                }
            }
            catch
            {
                ResultDiscardedGetter = null;
            }

            ResultDiscardedGetter = ResultDiscardedGetter ?? (_ => false);
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
    }
}