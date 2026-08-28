namespace StoreCommerce.Domain.Entity;

public class CustomerEmail
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    public CustomerEmail() { }

    public CustomerEmail(string value)
    {
        Value = value;
    }
    #endregion
}
