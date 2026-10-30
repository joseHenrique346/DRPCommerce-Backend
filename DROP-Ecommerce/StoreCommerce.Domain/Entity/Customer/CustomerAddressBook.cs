namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Regras do conjunto de endereços de um cliente: limite, um único padrão e reeleição do padrão ao excluir.
/// </summary>
public sealed class CustomerAddressBook(IReadOnlyList<CustomerAddress> addresses)
{
    public IReadOnlyList<CustomerAddress> Addresses => addresses;

    public CustomerAddress? Default => addresses.FirstOrDefault(address => address.IsDefault);

    /// <summary>
    /// Inclui o endereço. O primeiro endereço do cliente vira o padrão automaticamente.
    /// </summary>
    public void Add(CustomerAddress address, bool makeDefault)
    {
        if (addresses.Count >= CustomerAddress.MaxPerCustomer)
            throw new ArgumentException($"O cliente pode ter no máximo {CustomerAddress.MaxPerCustomer} endereços.");

        if (makeDefault || addresses.Count == 0)
            MakeDefault(address);
    }

    public void MakeDefault(CustomerAddress address)
    {
        foreach (var other in addresses.Where(item => !ReferenceEquals(item, address)))
            other.SetDefault(false);

        address.SetDefault(true);
    }

    /// <summary>
    /// Remove o endereço do conjunto. Se era o padrão, o endereço mais recente que restar passa a ser o padrão.
    /// </summary>
    public void Remove(CustomerAddress address)
    {
        if (!address.IsDefault)
            return;

        address.SetDefault(false);
        var next = addresses.Where(item => !ReferenceEquals(item, address)).OrderByDescending(item => item.Id).FirstOrDefault();
        next?.SetDefault(true);
    }
}
