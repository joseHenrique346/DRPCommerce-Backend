# Cancelamento e estorno recuperáveis

O pedido é cancelado localmente e devolve estoque/cupom na mesma transação que grava a intenção financeira em `operacao_pagamento`. O HTTP de cancelamento confirma essa alteração local; não comprova que o dinheiro já foi devolvido. Nenhuma chamada ao provedor ocorre nessa transação ou enquanto o catálogo está bloqueado.

`PaymentOperation` congela tipo, transação de origem, tentativa, conta vendedora, ambiente, conexão, valor, moeda disponível, motivo, origem/operador e chave de idempotência. Há unicidade por transação/tipo e por chave. Pedidos antigos pagos sem lançamento/contexto ficam em revisão; não são considerados estornados automaticamente.

## Os quatro passos

1. `OrderLifecycleService` grava a solicitação junto com o cancelamento/expiração ou a captura tardia. Repetições não devolvem estoque/cupom nem criam outra solicitação.
2. `PaymentOperationReconciliationWorker` seleciona até 50 operações a cada 30 segundos. Cada processamento usa um escopo novo. Um claim atômico com versão/lease de dois minutos é confirmado antes de chamar o adapter. A chamada tem timeout de 60 segundos e ocorre sem transação aberta.
3. O resultado é persistido sob uma transação curta. Sucesso de estorno grava o lançamento financeiro e atualiza o pagamento de origem atomicamente. O pedido só fica integralmente estornado quando não restam pagamentos capturados. Timeout/exceção não significa recusa nem sucesso.
4. `Processing` abandonado, `Pending` e `OutcomeUnknown` chamam somente `RecoverOperationAsync`, com a mesma correlação e o contexto original. Um worker antigo não pode sobrescrever um novo lease. Se a resposta chegou mas a gravação falhou, a próxima execução também consulta; não repete o POST automaticamente.

## Estados e ações

| Estado | Significado / ação |
| --- | --- |
| Requested (1) | Primeira execução autorizada. |
| Processing (2) | Claim em andamento; lease vencida permite somente consulta. |
| Pending (3) | Provedor aceitou e ainda não confirmou conclusão; consultar novamente. |
| Succeeded (4) | Conclusão confirmada; para estorno, lançamento financeiro persistido junto. |
| Failed (5) | Provedor comprovou recusa sem efeito financeiro. Nova tentativa exige ação explícita e conserva a chave. |
| OutcomeUnknown (6) | Resultado incerto; apenas consulta, com intervalo mínimo de um minuto. |
| AwaitingManualConfirmation (7) | Gateway manual aguarda operador confirmar devolução feita fora do sistema. |
| ReviewRequired (8) | Contexto/identificador insuficiente ou adapter indisponível. Resolver a causa; não inventar uma cobrança nem afirmar devolução. |
| Superseded (9) | Captura tardia substituiu o cancelamento remoto por solicitação de estorno. |

Cancelar antes da criação da cobrança usa CAS para passar a tentativa ainda `Requested` para `Aborted` (5). Se o worker já iniciou a criação, ela continua sendo recuperada por consulta; o cancelamento aguarda os identificadores persistidos. Uma aprovação tardia mantém o pedido cancelado, registra o recebimento e solicita estorno sem nova baixa/devolução de estoque.

## API e modo manual

O detalhe existente (`GET /api/orders/{id}/detail` e detalhe da vitrine) acrescenta `paymentOperations`: `id`, `kind`, `status`, `amount`, `currencyCode`, `reference`, `resolutionCode`, `completedAt`. Os enums seguem a serialização já configurada pela API. O frontend deve exibir cancelamento e andamento financeiro separadamente e consultar novamente enquanto houver pendências.

| Método | Rota | Uso |
| --- | --- | --- |
| POST | `/api/orders/{id}/payment-operations/{operationId}/retry` | Reagenda apenas `Failed`. Sem body; preserva a chave. Não força nova chamada para resultado desconhecido. |
| POST | `/api/orders/{id}/payment-operations/{operationId}/confirm-manual-refund` | Body `{ "reference": "comprovante-123", "note": "Devolução conferida" }`. Exclusivo de estorno manual aguardando confirmação. |

Os dois comandos exigem `orders.cancel` e `orders.refund`, usam a loja/operador autenticados e verificam o vínculo pedido/operação. Confirmações manuais repetidas não duplicam lançamento. O operador, a referência e a observação ficam registrados. No modo manual, cancelar a cobrança não movimenta dinheiro; estornar não é sucesso automático. O endpoint de confirmar recebimento também recusa cobranças de provedores externos.

Cancelamentos e estornos não concluídos, inclusive falha, revisão e confirmação manual, bloqueiam a exclusão da conta do comprador. O histórico financeiro permanece após a exclusão.

## Contrato do adapter real

O adapter recebe `PaymentOperationRequest`, incluindo `PaymentProviderContext`, IDs separados e chave estável. Deve autenticar na conexão original, conferir recebedor/ambiente/valor/moeda e interpretar o estado financeiro autoritativo. Não deve usar a conexão de outra conta ou o ambiente atual da loja para resolver uma operação antiga.

Para estorno, é obrigatório um `PaymentId` previamente verificado e correspondente à referência do pagamento capturado. A preferência de Checkout Pro não é aceita como substituto. Um `Succeeded` exige referência verificável; `Failed` exige comprovação de que nada foi aplicado. Ausência de resposta, resposta inválida e dúvida ficam incertas. Se a consulta não permite correlacionar um estorno inequivocamente, manter pendente/em revisão; não deduzir sucesso apenas pelo mesmo valor ou repetir o POST.

O [adapter Mercado Pago](MercadoPago.md) implementa o estorno integral com PaymentId e chave estável, seguido de consulta financeira na recuperação. Configuração e homologação externas são necessárias antes da ativação. Referências: [POST de estorno](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-api-payments/create-refund/post) e [consulta de estornos](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-api-payments/get-refunds/get).

## Banco e validação

Aplicar `AddRecoverablePaymentOperations` antes de executar a nova API/worker. A migração cria apenas a estrutura, sem conectar provedores. Seu rollback recusa descartar uma tabela com operações, para preservar as intenções e o histórico; correções posteriores devem manter esses registros.

Validação em PostgreSQL temporário isolado: 49 verificações do novo fluxo, incluindo checkout real com estoque/cupom, criação concorrente de solicitações, confirmação manual concorrente, persistência antes da chamada, catálogo liberado durante I/O externo, timeout, falha de gravação após sucesso externo, recuperação por consulta, lease abandonada/antiga, worker concorrente, retry explícito com a mesma chave, criação abortada, autorização pendente, aprovação tardia repetida, IDs insuficientes, legado, isolamento por loja e bloqueio de exclusão de conta. Também passaram as 42 verificações anteriores de conexões/checkout e 52 de exclusão de conta. Nenhuma movimentação real ou migração em banco existente do usuário.

OAuth, adapter e webhook estão implementados no backend. Próximo passo: configurar o [ambiente Mercado Pago](MercadoPago.md), adaptar o frontend e homologar a venda completa em sandbox.
