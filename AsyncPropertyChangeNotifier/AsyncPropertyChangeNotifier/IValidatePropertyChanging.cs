namespace System.ComponentModel
{
    public interface IValidatePropertyChanging
    {
        bool PropertyCanChange(string propertyName, object potentialValue);
    }
}
