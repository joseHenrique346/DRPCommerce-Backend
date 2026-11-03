using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Endereço de entrega salvo pelo cliente. O cliente pode ter vários e um é o padrão do checkout.
/// O pedido copia o endereço na compra, então alterar ou excluir um endereço não muda pedidos já feitos.
/// </summary>
public class CustomerAddress : BaseEntity, ITenantEntity
{
    #region Constants
    public const int MaxPerCustomer = 20;
    public const int LabelMaxLength = 50;
    public const int RecipientNameMaxLength = 200;
    // Mesmos limites do endereço de entrega do pedido, para o endereço sempre caber no pedido.
    public const int AddressLineMaxLength = Order.ShippingAddressLineMaxLength;
    public const int CityMaxLength = Order.ShippingCityMaxLength;
    public const int ZipCodeMaxLength = Order.ShippingZipCodeMaxLength;
    public const int CountryMaxLength = 100;
    #endregion

    #region Properties
    public long EnterpriseId { get; private set; }
    public long CustomerId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string RecipientName { get; private set; } = string.Empty;
    public string AddressLine { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public long StateId { get; private set; }
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }

    #region Navigation Properties
    public Customer Customer { get; private set; } = null!;
    public StaticEntity.State State { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructor
    protected CustomerAddress() { }
    #endregion

    #region Functions
    public static CustomerAddress Create(Customer customer, string label, string recipientName, string addressLine, string city, long stateId,
        string zipCode, string country)
    {
        BaseValidate.ValidateNotNull(customer, nameof(Customer));

        var address = new CustomerAddress
        {
            EnterpriseId = customer.EnterpriseId,
            CustomerId = customer.Id
        };
        address.UpdateDetails(label, recipientName, addressLine, city, stateId, zipCode, country);

        return address;
    }

    public void UpdateDetails(string label, string recipientName, string addressLine, string city, long stateId, string zipCode, string country)
    {
        BaseValidate.ValidateMaxLength(label, LabelMaxLength, nameof(Label));
        BaseValidate.ValidateMaxLength(recipientName, RecipientNameMaxLength, nameof(RecipientName));
        BaseValidate.ValidateNotNullOrEmpty(addressLine, nameof(AddressLine));
        BaseValidate.ValidateMaxLength(addressLine, AddressLineMaxLength, nameof(AddressLine));
        BaseValidate.ValidateNotNullOrEmpty(city, nameof(City));
        BaseValidate.ValidateMaxLength(city, CityMaxLength, nameof(City));
        BaseValidate.ValidatePositive(stateId, nameof(StateId));

        if (!StaticEntity.BaseStaticEntity.GetAll<StaticEntity.State>().Any(state => state.Id == stateId))
            throw new ArgumentException($"{nameof(StateId)} is invalid.");

        BaseValidate.ValidateNotNullOrEmpty(zipCode, nameof(ZipCode));
        BaseValidate.ValidateMaxLength(zipCode, ZipCodeMaxLength, nameof(ZipCode));
        BaseValidate.ValidateNotNullOrEmpty(country, nameof(Country));
        BaseValidate.ValidateMaxLength(country, CountryMaxLength, nameof(Country));

        Label = label?.Trim() ?? string.Empty;
        RecipientName = recipientName?.Trim() ?? string.Empty;
        AddressLine = addressLine.Trim();
        City = city.Trim();
        StateId = stateId;
        ZipCode = zipCode.Trim();
        Country = country.Trim();
    }

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
    #endregion
}
