using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    internal static class InternalUtilities
    {
        public static async Task InvokeInParallel(
            this PropertyChangedEventHandler handler
          , object me
          , string propertyName)
        {
            if (handler != null)
            {
                var args = new PropertyChangedEventArgs(propertyName);
                var tasks = from h in handler.GetInvocationList()
                                             .OfType<PropertyChangedEventHandler>()
                            select Task.Run(() => h(me, args));

                await Task.WhenAll(tasks);
            }
        }

        public static void FireAndForget(
            this PropertyChangedEventHandler handler
          , object me
          , string propertyName)
        { Task.Run(() => handler.InvokeInParallel(me, propertyName)); }

        public static async Task InvokeInParallel(
            this NotifyCollectionChangedEventHandler handler
          , object me
          , NotifyCollectionChangedEventArgs args)
        {
            if (handler != null)
            {
                var tasks = from h in handler.GetInvocationList()
                                             .OfType<NotifyCollectionChangedEventHandler>()
                            select Task.Run(() => h(me, args));

                await Task.WhenAll(tasks);
            }
        }

        public static void FireAndForget(
           this NotifyCollectionChangedEventHandler handler
         , object me
         , NotifyCollectionChangedEventArgs args)
        { Task.Run(() => handler.InvokeInParallel(me, args)); }

        public static void Replace(this NotifyCollectionChangedEventHandler handler, object me, int index, object item)
        {
            if (handler != null)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace
                  , new[] { item }
                  , index);

                handler.FireAndForget(me, args);
            }
        }

        public static void Add(this NotifyCollectionChangedEventHandler handler, object me, int index, params object[] items)
        {
            if (handler != null)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add
                  , new[] { items }
                  , index);

                handler.FireAndForget(me, args);
            }
        }
    }
}
