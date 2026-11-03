# Store — Infrastructure

O Store usa PostgreSQL com EF Core/Npgsql, banco próprio e StoreCommerceDbContext. Repositórios, UnitOfWork e configurações ficam em `StoreCommerce.Infrastructure.Data`; bibliotecas de segurança e Shared complementam a composição.

Os arquivos parciais do DbContext protegem identidade, inventário, carrinho, endereços, workflow de pedidos e aparência. `SaveChanges` valida tenant/relações/escopos; escrever diretamente em saldo ou pagamento não é um caminho alternativo aos casos de uso.

Workers expiram pedidos pendentes e reservas avulsas separadamente. O adapter registrado de pagamento é `ManualPaymentGateway`. Mercado Pago, Melhor Envio e emissão fiscal continuam pendentes.

- [Persistência, transações e migrations](../../Shared/Infrastructure.md)
- [Configuração e inicialização](../../Shared/StartupProject.md)
- [Estoque e concorrência](../Application/ProductsAndInventory.md)
- [Checkout e expiração](../Application/OrdersAndCheckout.md)
- [Cupons e ajuste dos compromissos existentes](../Application/Coupons.md)
