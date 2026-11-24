using DropCommerce.Application.Contracts;
using DropCommerce.Application.Features.Commands;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Features.Commands;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class CommercialInputValidationTests
{
    [Fact]
    public void Catalog_validator_rejects_duplicate_skus_and_malformed_options_without_persistence()
    {
        var validator = new CreateDropCatalogProductCommandValidator();
        var duplicates = new CreateDropCatalogProduct("Produto", "Descrição",
        [
            new("SKU", new() { ["color"] = "blue" }, 20, 1),
            new("sku", new() { ["color"] = "red" }, 20, 1)
        ]);
        Assert.False(validator.Validate(new CreateDropCatalogProductCommand(duplicates)).IsValid);

        var invalidOptions = new CreateDropCatalogProduct("Produto", "Descrição",
            [new("SKU", new() { ["color"] = "" }, 20, 1)]);
        Assert.False(validator.Validate(new CreateDropCatalogProductCommand(invalidOptions)).IsValid);
    }

    [Fact]
    public void Reservation_validator_rejects_repeated_variants_and_accepts_valid_structure()
    {
        var validator = new ReserveDropPurchaseCommandValidator();
        var repeated = new ReserveDropPurchase(1, 1, "reservation", [new(1, 1), new(1, 2)]);
        Assert.False(validator.Validate(new ReserveDropPurchaseCommand(1, repeated)).IsValid);

        var valid = new ReserveDropPurchase(1, 1, "reservation", [new(1, 1), new(2, 2)]);
        Assert.True(validator.Validate(new ReserveDropPurchaseCommand(1, valid)).IsValid);
    }

    [Fact]
    public void Concentration_is_allowed_while_a_nonpositive_purchase_limit_is_invalid()
    {
        var validator = new AssociateDropCatalogProductCommandValidator();
        Assert.True(validator.Validate(new AssociateDropCatalogProductCommand(1, new(1, 1_000_000))).IsValid);
        Assert.False(validator.Validate(new AssociateDropCatalogProductCommand(1, new(1, 0))).IsValid);
    }

    [Fact]
    public void Sale_payment_validator_checks_money_and_currency_without_calling_a_provider()
    {
        var validator = new StartDropSalePaymentCommandValidator();
        var input = new StartDropSalePayment(Guid.NewGuid(), 1, 1, 20, "BRL", DateTime.UtcNow.AddMinutes(15));
        Assert.True(validator.Validate(new StartDropSalePaymentCommand(1, input)).IsValid);
        Assert.False(validator.Validate(new StartDropSalePaymentCommand(1, input with { Amount = 20.001m })).IsValid);
        Assert.False(validator.Validate(new StartDropSalePaymentCommand(1, input with { Currency = "USD" })).IsValid);
        Assert.False(validator.Validate(new StartDropSalePaymentCommand(1, input with
        {
            ExpiresAt = DateTime.SpecifyKind(input.ExpiresAt, DateTimeKind.Unspecified)
        })).IsValid);
    }

    [Fact]
    public void Stock_transfer_validators_reject_missing_key_and_invalid_destination()
    {
        var start = new StartDropStockTransferCommandValidator();
        Assert.True(start.Validate(new StartDropStockTransferCommand(1, new("transfer", 1))).IsValid);
        Assert.False(start.Validate(new StartDropStockTransferCommand(1, new(" ", 1))).IsValid);

        var acknowledgment = new AcknowledgeDropStockTransferCommandValidator();
        Assert.True(acknowledgment.Validate(new AcknowledgeDropStockTransferCommand(1, Guid.NewGuid(), 1)).IsValid);
        Assert.False(acknowledgment.Validate(new AcknowledgeDropStockTransferCommand(1, Guid.NewGuid(), 0)).IsValid);
    }

    [Fact]
    public void Import_validator_checks_variant_options_before_any_stock_is_imported()
    {
        var validator = new ImportDropStockCommandValidator();
        var input = new DropStockExport(Guid.NewGuid(), 1, 1, "Produto", "Descrição",
            [new(1, "SKU", "{}", 20, 1)]);
        Assert.True(validator.Validate(new ImportDropStockCommand(input)).IsValid);
        Assert.False(validator.Validate(new ImportDropStockCommand(input with
        {
            Variants = [new(1, "SKU", "[]", 20, 1)]
        })).IsValid);
        Assert.False(validator.Validate(new ImportDropStockCommand(input with
        {
            Variants = [new(1, "SKU", "{", 20, 1)]
        })).IsValid);
    }

    [Fact]
    public void Queue_validator_checks_cpf_and_reactivation_validator_checks_mode_without_a_database()
    {
        var queue = new JoinDropQueueCommandValidator();
        Assert.True(queue.Validate(new JoinDropQueueCommand(1, new() { CPF = "529.982.247-25" })).IsValid);
        Assert.False(queue.Validate(new JoinDropQueueCommand(1, new() { CPF = "11111111111" })).IsValid);

        var preview = new PreviewDropReactivationQueryValidator();
        Assert.True(preview.Validate(new PreviewDropReactivationQuery(new(1, "oldest", null))).IsValid);
        Assert.False(preview.Validate(new PreviewDropReactivationQuery(new(1, "automatic", null))).IsValid);
    }
}
