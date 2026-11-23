# Arquitetura dos fluxos comerciais

A Parte 7 foi commitada localmente em `013fdbf`. A correção posterior das Partes 5 e 6
preserva as rotas e respostas de sucesso e reorganiza a execução neste fluxo:

```text
Controller → Command/Query → Validator → Handler
                                          ↓
                                  repositórios e adapters
                                          ↓
                                  Infrastructure.Data
```

Os endpoints de assinatura SaaS, recebimento do webhook, catálogo DROP, fila, reserva,
checkout, entrega, transferência de estoque, sincronização comercial e vitrine do DROP
enviam requests pelo MediatR. Os requests, handlers e validadores ficam em
`Features/Commands/<Contexto>/<Feature>/<Operação>`, com consultas dentro de `Queries`.
Os contratos de entrada/saída pertencem a Application, com um tipo público por arquivo.
Os retornos anônimos dos fluxos controlados foram substituídos por DTOs nomeados.

Os handlers Store reutilizam `IdentityHandlerBase`. Os handlers DROP implementam
`IRequestHandler` para os casos de uso específicos. Não foram acrescentados hooks aos
bases genéricos de CRUD. Esses fluxos coordenam autenticação, idempotência, transações e
integrações entre serviços; tratá-los como criação/alteração livre de uma entidade
permitiria ignorar essas etapas.

Os handlers contêm as regras e a coordenação dos casos de uso. Foram removidos os dez
Services de orquestração introduzidos nas Partes 5 e 6 e o auxiliar
`AccessContractPolicyService`, cuja regra passou a `ValidateEnterpriseTeamSelectionQueryHandler`.
Os handlers de cobrança ficam em `Features/Commands/Billing/PlatformBilling`; venda DROP,
transferência, catálogo, inscrição, fila e pedidos têm pastas próprias sob `Features/Commands`.
As implementações de repositórios ficam em `Infrastructure.Data/Repositories`.

A estrutura de pastas atual está descrita em
[Organização do projeto](OrganizacaoDoProjeto.md). A revisão de organização separa
tipos e contextos; ela não encerra os desvios de responsabilidade dos Services
existentes nem amplia o alcance das afirmações sobre as Partes 5 e 6 acima.

## Transferência dos Services para handlers — etapas 1 a 3

Após a organização, as operações de carrinho, endereços, contratos de produtos,
estoque, mídia, checkout, ciclo do pedido e processamento de pagamentos foram
transferidas dos Services de Application para seus respectivos handlers.
As permissões, verificações, sequência de gravações, locks e transações foram
preservados. A confirmação de um pedido sem cobrança continua acontecendo depois
da gravação inicial, e chamadas ao provedor continuam fora dessa transação inicial.

Os fragmentos compartilhados de baixa/devolução de estoque, uso de cupons,
enfileiramento de operações financeiras e conclusão/expiração de reservas usam
Writers específicos. Esses Writers não recebem o caso de uso inteiro, não
despacham outros handlers e não confirmam a transação do chamador. Seleção de
reativação, leitura de histórico e projeção do carrinho usam auxiliares próprios.

Controllers e workers que chamavam diretamente um Service removido passaram a
acionar o handler correspondente. Essa adaptação preserva as rotas e os formatos
de resposta; não representa uma revisão geral dos controllers ou workers.

O adapter de credenciais `ProviderConnectionService` permanece em Integrations,
com proteção, persistência técnica e renovação de tokens. Os fluxos de OAuth e
webhook existentes na infraestrutura ainda precisam da revisão arquitetural já
registrada anteriormente.

A revisão geral de `ProductCatalogWriter`, `ProductVariantWriter` e das composições
entre handlers pertence à etapa 4 e permanece pendente. Os bases existentes de
CRUD e Identity não foram alterados nesta transferência. Regras comerciais
anteriores em repositórios e DbContexts também não foram redistribuídas aqui.

Workers e reconciliação acionam Commands/Queries pelo MediatR. Operações compartilhadas,
como reconciliar uma assinatura, expirar reservas e aplicar um recibo de pagamento,
têm handlers próprios. A prévia e a confirmação de reativação do DROP são implementadas pelo
mesmo handler, com a seleção calculada sob lock. Não há um Service intermediário
recebendo a execução encaminhada pelo handler.

Validators verificam formato e estrutura de entrada: catálogo/variações, opções JSON,
linhas da reserva, chave idempotente, valor/moeda, CPF, modo de reativação, tópico de
notificação e identificação do destino de estoque. Entrada inválida usa a resposta 400
do pipeline existente; permissões, revisão, cotas, disponibilidade, prazo restante,
idempotência e consistência financeira são conferidos pelos handlers.

As gravações reutilizam os units of work existentes. As consultas especializadas,
bloqueios SQL, transações de banco, deduplicação da inbox e validação criptográfica da
assinatura têm interfaces em Application e implementações na infraestrutura. O adaptador
comum `DropCommercePersistence` compartilha apenas essas operações técnicas entre os
repositórios DROP. Ele não executa compra, cobrança, admissão ou transferência de estoque.
`ICurrentDropActor` fornece o contexto autenticado sem levar HttpContext para os handlers.
`DropPurchaseIdentity` é um helper técnico de normalização/validação do CPF e HMAC;
não executa fila, reserva ou compra. `MediatorResultExtensions` apenas propaga o resultado
tipado ou a falha de um request interno, preservando os códigos de erro. Esses auxiliares
não contêm orquestração de casos de uso e não acrescentam hooks aos bases.

Os guards dos DbContexts continuam como proteção da persistência. Foram preservados os
filtros de tenant/cliente, os bloqueios, a sequência de commits, as revisões e os snapshots.
Os endpoints públicos conservam seus corpos simples e os endpoints internos conservam o
envelope `Result<T>`. A reorganização não acrescenta migrations nem altera tabelas.

`CommercialArchitectureTests` verifica as dependências de Application, a entrada dos
controllers pelo MediatR, a existência de um único handler por request e os campos JSON
dos DTOs e a ausência dos Services removidos. `CommercialInputValidationTests` exercita
os validators sem banco/provedor. `CommercialArchitectureApiTests` verifica inscrição pela
API, idempotência e isolamento entre compradores e lojas. A suíte comercial continua exercitando pagamento,
fila, estoque, downgrade, carência e reconciliação com provedores fictícios.

O roteiro de execução e os limites da homologação estão em
[Validação comercial para o TCC](ValidacaoComercialTcc.md).
