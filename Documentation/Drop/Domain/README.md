# Drop — Domain

O domínio do Drop contém eventos e seu catálogo (DropEvent/DropProduct/DropCoupon), inscrições, QueueEntry/QueueSession, lista de espera, reservas, pedidos/itens/transações, notificações, sinais de fraude e auditoria.

Produtos, clientes, funcionários e lojas usam IDs canônicos do Store, sem FK entre bancos. O vínculo de tenant e a propriedade dos recursos são conferidos na leitura/escrita e na integração interna. Tokens de fila/sessão não são dados públicos do catálogo.

A existência dessas entidades não representa uma máquina de admissão e venda completa. DropOrder/DropTransaction ainda não estão ligados automaticamente ao checkout/pagamento do Store; as regras para correlação única e compromisso de SKU permanecem pendentes.

- [Casos de uso atuais e pendências](../Application/README.md)
- [Isolamento e integração Store/Drop](../Infrastructure/StoreIntegration.md)
- [Modelo canônico de identidade](../../Store/Domain/Identity.md)
