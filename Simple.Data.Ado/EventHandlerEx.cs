namespace Shitty.Data.Ado
{
    using System;

    internal static class EventHandlerEx
    {
        public static T Raise<T>(this EventHandler<T> handler, object sender, Func<T> args)
            where T : EventArgs
        {
            if (handler != null)
            {
                var e = args();
                handler(sender, e);
                return e;
            }
            return null;
        }
    }
}