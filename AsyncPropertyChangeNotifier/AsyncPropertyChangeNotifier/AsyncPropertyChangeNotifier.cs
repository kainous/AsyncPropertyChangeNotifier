using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace System.ComponentModel
{
    public class AsyncPropertyChangeNotifier
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BlockingCollection<IValidatePropertyChanging> _PropertyChangingSubscribers
            = new BlockingCollection<IValidatePropertyChanging>();

        public void SubscribeToPropertyChangingValidation(IValidatePropertyChanging subscriber)
        {
            _PropertyChangingSubscribers.Add(subscriber);


        }

        public void UnsubscribeFromPropertyChangingValidation(IValidatePropertyChanging subscriber)
        {
            _PropertyChangingSubscribers.t
            }

        protected virtual async Task<bool> CanPropertyChange(
            object potentialValue
          , [CallerMemberName] string propertyName = null)
        {
            foreach (var subscriber in _PropertyChangingSubscribers)
            {

            }

            var args = new CancelablePropertyChangingEventArgs(propertyName, potentialValue);
            var tasks = PropertyChanging.GetInvocationList()
                                        .OfType<EventHandler<CancelablePropertyChangingEventArgs>>()
                                        .Select(h =>
                                        {
                                            h(this, args);
                                            args.
                                        })



                from handler in PropertyChanging.GetInvocationList().OfType<EventHandler<CancelablePropertyChangingEventArgs>>()
                select Task.Run(() => handler(this, args));

            tasks.AsParallel()



            foreach (var handler in PropertyChanging.GetInvocationList())
            {

            }

            PropertyChanging?.Invoke(
                propertyName
              , new CancelablePropertyChangingEventArgs(propertyName, potentialValue));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var d = new CancelablePropertyChangingEventArgs(propertyName, ;
            d.PropertyName

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
