namespace System.ComponentModel
{
    public interface IDataChangeEvent<T>
    {
        DateTimeOffset Timestamp { get; }
        T Value { get; }
    }
}
