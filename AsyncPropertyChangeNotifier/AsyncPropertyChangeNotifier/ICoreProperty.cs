using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public class DataChangeEvent<T> : IDataChangeEvent<T>
    {
        public DateTimeOffset Timestamp { get; }
        public T Value { get; }

        public DataChangeEvent(T value, DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
            Value = value;
        }

        public DataChangeEvent(T value) : this(value, DateTimeOffset.Now) { }
    }

    public interface IDataChangeEvent<T>
    {
        DateTimeOffset Timestamp { get; }
        T Value { get; }
    }

    public class CoreProperty<T> : ICoreProperty<T>
    {
        public string PropertyName { get; }

        public IDataChangeEvent<T> Recent { get; set; }

        public CoreProperty(string propertyName, T defaultValue)
        {
            PropertyName = propertyName;
            Recent = new DataChangeEvent<T>(defaultValue, DateTimeOffset.MinValue);
        }
    }

    public interface ICoreProperty<T>
    {
        string PropertyName { get; }
        IDataChangeEvent<T> Recent { get; set; }
    }
}
