# DROP: catálogo, fila, estoque e venda

Parte 6, revisada em 21/11/2026 na branch de integração e commitada localmente em `0b02bb6`,
sem push, antes da Parte 7 de validação. A Parte 5 está no commit local `1a85fc1`.

A Parte 7 foi commitada localmente em `013fdbf`. A refatoração posterior das Partes 5 e 6
é descrita em [Arquitetura dos fluxos comerciais](../../Shared/ArquiteturaComercial.md).

## Responsabilidade dos serviços

O Store conserva identidade, contrato comercial e conexão Mercado Pago da loja. O DROP
tem catálogo próprio, estoque por variação, fila, reservas e pedidos em seu banco.
Produto próprio DROP não precisa existir no Store: `ProductId` é uma referência externa
opcional, nunca um ID fictício ou uma FK entre bancos.

Para vendas DROP, o Store funciona como ponte de pagamento com uma intenção financeira
própria. Não cria outro pedido Store nem reserva novamente estoque Store. A cobrança SaaS
da plataforma continua no fluxo independente `PlatformBilling` da Parte 5.

## Catálogo e contrato

- Todos os principais DROP não excluídos consomem a cota total de criação, mesmo sem evento.
  Variações não acrescentam vagas. Excluir preserva os registros históricos.
- Publicação/agendamento consome um par principal/evento. O mesmo principal em dois eventos
  consome duas vagas; seus SKUs dentro de um evento continuam sendo uma vaga.
- A projeção do contrato é monotônica por loja/revisão. Downgrade bloqueia excedentes por
  `CreatedAt, Id`, sem apagar dados, alterar `IsActive` ou remover estoque.
- Recuperar o plano não republica produtos automaticamente. Há reativação explícita do
  cadastro e prévia/confirmação das participações, com revisão esperada e capacidade atual.
- Catálogo, participação, aparência pública e admissão revalidam direitos. Falha de consulta
  ao Store não concede acesso. Reserva já existente pode concluir dentro do prazo original.
- `atividade_comercial` registra criação, bloqueio, reativação e importação, sem CPF/IP.
  A consulta exige a permissão própria `store.history.view`.

O catálogo novo aceita até 200 variações por principal, SKU de até 100 caracteres, quantidade
de até 1.000.000 por variação e preço positivo de até R$ 9.999.999.999,99. Estes são limites
técnicos de entrada, separados das cotas comerciais. Os preços vêm do servidor.

## Transferência de estoque

1. Um gestor com `products.edit` arquiva o produto Store e solicita a transferência com chave idempotente e revisão
   de contrato. Reservas, pedidos em andamento, pagamentos pendentes/incertos ou operações
   financeiras não resolvidas impedem a transferência inteira.
2. Na mesma transação Store, o saldo é retirado, movimentos são registrados, o produto fica
   marcado como controlado pelo DROP e uma intenção imutável guarda o snapshot original.
3. O worker importa uma vez no DROP e confirma a operação na origem. Não há intervalo em
   que ambos os bancos ofereçam aquelas unidades. Uma falha pode deixar saldo indisponível
   temporariamente; não autoriza recriar uma exportação.
4. Rejeição comercial é persistida no DROP antes de restaurar a origem. Esse registro impede
   que uma importação atrasada duplique o saldo depois do rollback da transferência.
5. Para retornar, o principal não pode participar de evento ativo/futuro, inclusive rascunho
   futuro. Reservas, pedidos e pagamentos incertos/em revisão também bloqueiam o retorno.
6. O DROP retira o restante e persiste uma intenção de devolução. O Store recebe uma vez,
   preserva seu produto arquivado e confirma; só então o DROP encerra a devolução.

O painel deve usar `isTransferredToDrop` dos produtos Store e o catálogo DROP para separar
os módulos. A origem arquivada permanece no histórico/cota total Store enquanto não excluída.
Transferência preserva a data original para a política de antiguidade.

## Fila e compra

O usuário confirmou: a franquia de CPF reinicia a cada evento; unidades de todas as variações
do mesmo principal somam no limite. Duas azuis e uma preta esgotam um limite de três.
Contas diferentes com o mesmo CPF também compartilham a franquia daquele principal/evento.

O CPF é validado e convertido em HMAC por loja/evento com uma chave privada. O texto original
não é persistido nem devolvido. IP e user-agent são sinais técnicos, não identidade de pessoa.
O mesmo cliente não troca o CPF dentro do evento. Não há prova documental de titularidade do CPF.

Entrada exige janela da fila, produto elegível e inscrição aprovada quando exigida. A sessão
é criada na admissão FIFO, não na entrada: tem **15 minutos fixos**. Repetir a entrada, reservar
outro SKU ou iniciar checkout não renova o prazo. O limite simultâneo padrão é 20 compradores
por evento, configurável; ele não representa uma capacidade já medida em teste de carga.

A reserva pertence a cliente/sessão/evento, usa chave idempotente, soma reservas vigentes e
vendas confirmadas do CPF e captura nome/SKU/opções/preços. Lock transacional por loja serializa
a disputa de cotas e saldo. Eventos diferentes compartilham o estoque físico do principal.

Na expiração, o saldo reservado é liberado e a entrada volta ao fim da fila, recebendo uma
nova sessão apenas ao ser admitida novamente. A expiração local não depende do Mercado Pago.
O backend permite limite de compra igual ao estoque inteiro e informa aviso de concentração
no detalhe público; cabe ao frontend apresentar a recomendação sem bloquear o dono.

## Pagamento e confirmação

O checkout cria primeiro o pedido DROP e os snapshots de comprador/endereço/itens. Um GUID
estável identifica a operação enviada ao Store. O Store persiste intenção e marca a submissão
antes de chamar o adapter Mercado Pago da loja, com correlação `drp-drop-...`.

Resposta perdida é recuperada por correlação; busca vazia não provoca outro POST de criação.
Preferência, recebedor, ambiente, valor e moeda ficam vinculados à intenção. O cliente recebe
somente a URL hospedada; retorno do navegador não confirma venda.

Webhooks assinados existentes e reconciliação consultam o provedor. A ponte valida os termos
com o adapter existente e publica recibos versionados ao DROP. Consumo de estoque e confirmação
da reserva/pedido ocorrem juntos e uma única vez no banco DROP.

**Aprovação observada após a expiração/liberação** registra `DropPayment.LateApproval` no pedido,
sem confirmar venda, consumir o saldo liberado ou estornar automaticamente. Reversão, cobrança
duplicada ou divergência exige revisão. Revisão financeira mantém o impedimento de exclusão de
conta/retorno de estoque. A ponte Store persiste `fulfillment_review_required`, reabre o
encerramento e impede que um ack antigo de fechamento remova esse bloqueio. O código ainda
não oferece resolução administrativa dessas pendências.

Pedidos pagos têm avanço administrativo para processamento, envio e entrega (3/4/5).
O Store recebe confirmação durável de encerramento para a elegibilidade de exclusão da conta.
Estado pendente ou resultado incerto não é tratado como prova de ausência de cobrança.

## Rotas públicas e administrativas

Os caminhos abaixo são dos serviços. No Gateway, acrescentar `/api/drop` ou `/api/store`
conforme o serviço, seguindo a composição já usada pelo projeto.

| Serviço | Método e caminho | Finalidade |
| --- | --- | --- |
| Store | `GET /api/enterprises/current/contract/drop-sync` | Dono consulta revisão e confirmação do contrato no DROP |
| Store | `POST /api/products/{id}/drop-stock-transfer` | Gestor com `products.edit` exporta estoque arquivado, com `idempotencyKey` e `expectedContractRevision` |
| DROP | `GET /api/drop-catalog/products?afterId=...` | Catálogo privado, até 50 principais por página, com variações/bloqueios |
| DROP | `POST /api/drop-catalog/products` | Cria principal próprio e variações com saldo inicial |
| DROP | `GET /api/drop-catalog/usage` | Cotas de cadastro/publicação, bloqueios e transferências |
| DROP | `POST /api/drop-catalog/products/{id}/events` | Associa principal a evento com `dropEventId` e `maxPerCustomer` |
| DROP | `POST /api/drop-catalog/products/{id}/reactivate` | Reativa cadastro com `expectedContractRevision`, sem republicar participações |
| DROP | `POST /api/drop-catalog/reactivation/preview` e `/reactivation` | Prévia/confirmação por antiguidade ou seleção de participações |
| DROP | `DELETE /api/drop-catalog/products/{id}` | Exclusão lógica após verificar reservas, estoque e operações financeiras |
| DROP | `POST /api/drop-catalog/products/{id}/return-to-store` | Inicia retorno coordenado, respondendo 202 |
| DROP | `GET /api/drop-catalog/activity?beforeId=...` | Histórico comercial privado, até 100 registros |
| DROP | `GET /api/storefront/{storeSlug}/drop/events` e `/events/{id}` | Eventos públicos elegíveis; detalhe inclui participações, SKUs, saldo e preços |
| DROP | `POST /api/storefront/{storeSlug}/drop/events/{id}/queue` | Entrada autenticada com `{ "cpf": "..." }` |
| DROP | `GET /api/storefront/{storeSlug}/drop/events/{id}/queue` | Posição/status e sessão própria com admissão/expiração |
| DROP | `POST /api/storefront/{storeSlug}/drop/events/{id}/reservations` | Reserva `participationId`, `sessionId`, `idempotencyKey`, `lines[{variantId,quantity}]` |
| DROP | `POST /api/storefront/{storeSlug}/drop/reservations/{id}/checkout` | Cria/recupera pedido e checkout da mesma reserva |
| DROP | `GET /api/storefront/{storeSlug}/drop/purchases/{id}` | Pedido próprio, snapshots, URL e eventual revisão financeira |
| DROP | `POST /api/drop-purchases/{id}/fulfillment` | Gestor com `orders.manage` avança `statusId` após pagamento confirmado |

CRUDs legados de fila/sessão/reserva/pedido/transação não podem gravar estados financeiros
ou saldos por fora destes workflows. Consultas privadas antigas continuam disponíveis.
Modelos de aparência que referenciem o catálogo novo usam `x-resource: drop_product`;
`product` mantém o significado antigo de ID Store, sem conversão silenciosa de identificadores.

## Integração interna e operação

Rotas Store exclusivas dos serviços, sempre com `X-Service-Key`; recursos também exigem
`X-Enterprise-Id` autenticado pelo middleware:

- `GET /api/internal/drop-contracts/pending`, `GET /api/internal/enterprises/{id}/drop-contract`
  e `POST /api/internal/enterprises/{id}/drop-contract/ack`.
- `GET /api/internal/drop-stock-transfers/pending`, `POST /api/internal/drop-stock-transfers/{operationId}/ack`
  e `POST /api/internal/drop-stock-transfers/return`.
- `POST /api/internal/drop-sale-payments`, `GET /api/internal/drop-sale-payments/{operationId}`,
  `GET /api/internal/drop-sale-payments/pending-receipts`, `POST /api/internal/drop-sale-payments/{operationId}/ack`
  e `GET /api/internal/drop-sale-payments/customers/{id}/profile`.

O ack financeiro informa `revision`, `fulfillmentClosed` e `requiresReview`. A revisão
financeira é persistente e não pode ser removida por outro ack de fechamento.

Não expor o prefixo interno no Gateway público. As listagens globais servem à reconciliação
autenticada; não aceitam escolher uma loja por header fornecido por comprador.

Configure no DROP `DropQueue__PurchaseIdentityKey` com segredo privado estável de pelo menos
32 caracteres e `DropQueue__ConcurrentBuyersPerEvent` entre 1 e 1000. Sem a chave, entrada
retorna 503. Não rotacionar a chave durante eventos em andamento sem migrar as identidades:
ela faz parte da franquia de CPF. Manter os bancos separados, a chave interna e a URL Store
já descritos na [integração](../Infrastructure/StoreIntegration.md).

Worker DROP: expiração/admissão a cada 10 segundos e contrato/estoque/recibos a cada 30.
Worker Store: consulta de intenções financeiras pendentes aproximadamente a cada minuto,
quando Mercado Pago está habilitado. Os workers precisam estar ativos para progresso sem
requisição do comprador. Lotes e locks locais não substituem teste de carga/observabilidade.

## Migrations e validação

Aplicar primeiro o Store atualizado e depois o DROP, com backup e revisão no ambiente escolhido:

- Store: `20261120153734_AddDropContractPublication`, `20261120155424_AddDropStockOwnership`,
  `20261120163011_AddDropSalePayments`, `20261120165042_AddDropFulfillmentReview`.
- DROP: `20261120153737_AddDropCommercialCatalog`, `20261120155428_AddDropStockTransferSaga`,
  `20261120163013_AddControlledDropPurchases`.

O legado preserva vínculos/histórico e ganha catálogo-base com saldo zero: contador antigo
não é convertido em estoque físico. É necessário conferir e transferir o estoque Store.
Sessões antigas são invalidadas, pois não comprovam admissão com CPF/prazo novo. Reservas e
pedidos históricos permanecem. Rollback com compras novas exige migração explícita do histórico.

Os testes usam PostgreSQL local descartável, HTTP interno de teste e gateway fictício.
Cobrem cotas, downgrade/reativação, concorrência, CPF, variações, fila, expiração, transferência
e rollback entre bancos, resposta perdida e aprovação tardia. Não houve cobrança real ou
homologação externa nesta etapa. As migrations não foram aplicadas a bancos existentes.

Verificação final em 21/11/2026: solução compilada sem erros e 171 testes aprovados na
suíte completa (156 anteriores e 15 novos). O cenário de revisão financeira valida também
o endpoint interno de ack e o bloqueio de exclusão da conta após um ack antigo de fechamento.

Permanecem fora desta etapa: frontend/painel, homologação real com contas de teste, integração
Melhor Envio/cálculo de frete, estorno/resolução financeira, verificação documental de CPF,
proteção distribuída contra abuso e medição de capacidade. O checkout DROP atual usa frete
zero; não representa cotação ou etiqueta de envio. O catálogo próprio oferece saldo inicial;
reposição/edição avançada de inventário ainda precisa de workflow específico, sem alterar
diretamente os saldos por CRUD.

A Parte 7 acrescenta [validação integrada e roteiro do TCC](../../Shared/ValidacaoComercialTcc.md),
com autenticação real entre as duas APIs, execução automatizada e relatórios fora do Git.
