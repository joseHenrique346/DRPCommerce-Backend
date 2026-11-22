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

Workers e reconciliação acionam Commands/Queries pelo MediatR. Operações compartilhadas,
como reconciliar uma assinatura, expirar reservas e aplicar um recibo de pagamento,
têm handlers próprios. A prévia e a confirmação de reativação são implementadas pelo
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
