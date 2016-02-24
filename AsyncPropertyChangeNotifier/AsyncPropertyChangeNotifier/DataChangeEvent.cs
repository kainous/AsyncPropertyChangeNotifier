namespace System.ComponentModel
{
    public struct DataChangeEvent<T> : IDataChangeEvent<T>
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
}
