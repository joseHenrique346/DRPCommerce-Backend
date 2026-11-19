using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DRP_CONTRACTS_TEST_POSTGRES")))
            Skip = "Configure DRP_CONTRACTS_TEST_POSTGRES para criar um banco local descartável.";
    }
}
