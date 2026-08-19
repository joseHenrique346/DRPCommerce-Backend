using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class SupplierEmail
{
    #region Properties
    public string Value { get; private set; }
    #endregion

    #region Constructor
    protected SupplierEmail() { }

    private SupplierEmail(string value)
    {
        Value = value;
    }
    #endregion

    #region Functions
    public static SupplierEmail Create(string value)
    {
        BaseValidate.ValidateNotNullOrEmpty(value, nameof(Value));
        BaseValidate.ValidateMaxLength(value, 255, nameof(Value));
        BaseValidate.ValidateEmailFormat(value, nameof(Value));

        return new SupplierEmail(value);
    }
    #endregion
}