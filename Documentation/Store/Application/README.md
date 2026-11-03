# Store — Application

Esta pasta documenta casos de uso, handlers, validações e contratos da camada Application do StoreCommerce.

- [Casos de uso de identidade](IdentityFlows.md): Commands/Queries, handlers, repositórios e transações de usuários, lojas e acessos.
- [Categorias do Store](Categories.md): regras de hierarquia, ativação, exclusão, busca e consultas da vitrine.
- [Produtos, variantes e estoque](ProductsAndInventory.md): catálogo por SKU, opções, reservas, movimentações e consultas.
- [Limite de usos dos cupons](Coupons.md): compromisso no checkout, confirmação e liberação de utilizações.
- [Carrinho, checkout, pedidos e clientes](OrdersAndCheckout.md): configurações, seleção, endereços, cobrança manual, numeração, histórico, cancelamento e expiração.

Os handlers comerciais trabalham com tenant/identidade autenticados, repositórios e escopos de gravação. Preço, estoque e estado de pagamento não são campos livres controlados pelo comprador. Os contratos HTTP estão em [Endpoints](../Api/Endpoints.md); integrações externas continuam descritas como [trabalho pendente](../../Shared/PlanejamentoIntegracoes.md).
