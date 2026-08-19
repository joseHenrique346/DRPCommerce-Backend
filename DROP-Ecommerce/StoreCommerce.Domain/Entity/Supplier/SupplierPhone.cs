using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class SupplierPhone
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    protected SupplierPhone() { }

    private SupplierPhone(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static SupplierPhone Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateLength(value, 7, 20, nameof(Value));
        BaseValidate.ValidatePhoneFormat(value, nameof(Value));

        return new SupplierPhone(value);
    }
    #endregion
}