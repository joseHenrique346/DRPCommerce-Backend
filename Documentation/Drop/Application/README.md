# Drop — Application

O módulo possui Commands/Queries, validators e contratos de leitores do Store para eventos, produtos, inscrições, fila, reservas e pedidos. Parte do storefront atual consulta/grava por controllers com DbContext; a organização não é idêntica à dos novos fluxos comerciais do Store.

## Implementado

- Inscrição e entrada autenticada na fila, com elegibilidade e janela do evento.
- Catálogo próprio, cotas comerciais e sincronização durável do contrato.
- Admissão FIFO, franquia de unidades por CPF/principal/evento e sessão fixa de quinze minutos.
- Reserva de variações com snapshots e compromisso do estoque DROP apenas na confirmação.
- Transferência exclusiva de estoque entre módulos, com confirmação e recuperação entre bancos.
- Checkout pela conexão Mercado Pago da loja, intenção durável e revisão de aprovação tardia.
- Consultas privadas limitadas ao cliente/loja; autenticação e recursos canônicos conferidos no Store.
- Resolução da aparência do evento/padrão, preview, publicação e fixação da revisão visual.

## Pendente

- Frontend, painel operacional da fila e homologação externa de pagamentos.
- Frete/Melhor Envio, resolução financeira/estorno e workflows avançados de reposição de estoque.
- Limitação/cache distribuídos e medição de capacidade para milhares de pessoas esperando.

O saldo físico tem controle exclusivo por módulo. Para transferir, o Store retira suas unidades
antes da importação DROP; não há cópia que mantenha o mesmo saldo disponível nos dois bancos.
Veja [Fluxo comercial DROP](FluxoComercial.md) para contratos, rotas, migrations e limites atuais.

Veja [integração Store/Drop](../Infrastructure/StoreIntegration.md) e [integrações externas pendentes](../../Shared/PlanejamentoIntegracoes.md).
