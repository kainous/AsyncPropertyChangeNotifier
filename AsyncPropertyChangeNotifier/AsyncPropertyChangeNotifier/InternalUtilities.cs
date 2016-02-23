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

        public static void Reset(this NotifyCollectionChangedEventHandler handler, object me)
        {
            if (handler != null)
            {
                var reset = NotifyCollectionChangedAction.Reset;
                var args = new NotifyCollectionChangedEventArgs(reset);
                handler.FireAndForget(me, args);
            }
        }

        private static IEnumerable<Tuple<int, int>> GetStartEnd<T>(IDictionary<int, T> items)
        {
            if (items == null)
            { yield break; }

            int start = int.MinValue;
            int end = int.MinValue;
            foreach (var key in items.Keys.OrderBy(o => o))
            {
                if (start == int.MinValue && end == int.MinValue)
                {
                    start = key;
                    end = key;
                }
                else if (key - 1 != start)
                {
                    yield return Tuple.Create(start, end);
                    start = key;
                    end = key;
                }
                else
                { end = key; }
            }

            if (start == int.MinValue && end == int.MinValue)
            { yield return Tuple.Create(start, end); }
        }

        public static void NotifyRemove<T>(this NotifyCollectionChangedEventHandler handler, object me, long itemCount, IDictionary<int, T> removedItems)
        {
            if ((handler == null) || (removedItems == null))
            { return; }

            var total = GetStartEnd(removedItems).ToArray();
            if (removedItems.Count / total.Length < 5) //20% change
            { handler.Reset(me); }
            else
            {
                foreach (var t in total)
                {
                    int start = t.Item1;
                    int count = t.Item2 - start;
                    var enu = Enumerable.Range(start, count);

                    var args = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove
                      , enu.Select(o => removedItems[o]).ToList()
                      , start);

                    handler(me, args);
                }
            }
        }
    }
}
