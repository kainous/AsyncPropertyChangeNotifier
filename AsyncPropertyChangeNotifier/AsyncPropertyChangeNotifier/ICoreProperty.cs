namespace System.ComponentModel
{
    public interface ICoreProperty<T>
    {
        string PropertyName { get; }
        IDataChangeEvent<T> Recent { get; set; }
    }
}
