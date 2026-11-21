# Drop — Domain

O domínio do Drop contém eventos e seu catálogo (DropEvent/DropProduct/DropCoupon), inscrições, QueueEntry/QueueSession, lista de espera, reservas, pedidos/itens/transações, notificações, sinais de fraude e auditoria.

Clientes, funcionários e lojas usam IDs canônicos do Store, sem FK entre bancos. Produtos DROP
podem ser próprios (`DropCatalogProduct`/`DropCatalogVariant`) ou ter origem Store opcional,
após transferência coordenada do estoque. `DropProduct` representa a participação em um evento.
O vínculo de tenant e a propriedade dos recursos são conferidos na leitura/escrita e na
integração interna. Tokens de fila/sessão e a identidade HMAC de CPF não são dados públicos.

O [fluxo comercial controlado](../Application/FluxoComercial.md) usa sessão/reserva/pedido próprios,
snapshots imutáveis e ponte financeira Store com correlação única. A intenção financeira não
cria outro pedido ou consumo de estoque Store. Confirmação tardia fica em revisão sem estorno
automático. Entidades legadas de transação não podem confirmar compra por CRUD genérico.

- [Casos de uso atuais e pendências](../Application/README.md)
- [Isolamento e integração Store/Drop](../Infrastructure/StoreIntegration.md)
- [Modelo canônico de identidade](../../Store/Domain/Identity.md)
