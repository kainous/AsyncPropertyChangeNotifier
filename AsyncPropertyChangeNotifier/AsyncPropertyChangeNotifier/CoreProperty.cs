namespace System.ComponentModel
{
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
}
