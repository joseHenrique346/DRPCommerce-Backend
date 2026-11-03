# Store — Domain

O Store é a fonte de lojas, identidade, catálogo por SKU e venda comercial. Revisado em 03/11/2026.

- [Identidade e autorização](Identity.md): User, Customer, Enterprise, Membership, cargos e perfis de acesso.
- Catálogo: Product é o pai; ProductVariant é o SKU vendável e dono do estoque. Category governa a árvore e a visibilidade herdada.
- Venda: Cart/CartItem preservam seleção; Order/OrderItem guardam snapshots; CustomerAddress fornece endereço; OrderStatusHistory registra transições.
- Compromissos: StockReservation/StockReservationLine comprometem saldo; StockMovement registra movimentação; CouponRedemption ocupa uso limitado de cupom, inclusive em pedido pendente.
- Operação: StoreSettings governa limites/backorder/prazo; Transaction, Shipment e Invoice representam cobrança/envio/documento, sem comprovar integração externa pronta.

Estados e classificações usam os IDs estáticos do domínio. Regras dependentes de vários agregados são coordenadas pela Application e protegidas novamente na persistência.

Regras detalhadas: [categorias](../Application/Categories.md), [catálogo/estoque](../Application/ProductsAndInventory.md), [cupons](../Application/Coupons.md) e [checkout/pedidos](../Application/OrdersAndCheckout.md).
