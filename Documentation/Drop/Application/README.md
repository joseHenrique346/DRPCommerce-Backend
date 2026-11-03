# Drop — Application

O módulo possui Commands/Queries, validators e contratos de leitores do Store para eventos, produtos, inscrições, fila, reservas e pedidos. Parte do storefront atual consulta/grava por controllers com DbContext; a organização não é idêntica à dos novos fluxos comerciais do Store.

## Implementado

- Inscrição e entrada autenticada na fila, com elegibilidade e janela do evento.
- Entrada na fila sob transação/bloqueio do evento; repetição normal retorna a entrada existente do cliente.
- Consultas privadas limitadas ao cliente/loja; autenticação e recursos canônicos conferidos no Store.
- Resolução da aparência do evento/padrão, preview, publicação e fixação da revisão visual.

## Pendente

- Admissão controlada e expiração comercial completa da sessão.
- Reserva/consumo do mesmo SKU do Store e conversão em venda sem duas cobranças.
- Reconciliação entre bancos, recuperação de falhas e painel operacional da fila.
- Limitação/cache distribuídos e medição de capacidade para milhares de pessoas esperando.

Não copiar o estoque para um contador independente no Drop como se ele fosse o saldo físico autoritativo. O Store já dispõe de [variantes/inventário](../../Store/Application/ProductsAndInventory.md) e [checkout/pedidos](../../Store/Application/OrdersAndCheckout.md), mas os comandos internos de integração dessa venda ainda precisam ser definidos.

Veja [integração Store/Drop](../Infrastructure/StoreIntegration.md) e [integrações externas pendentes](../../Shared/PlanejamentoIntegracoes.md).
