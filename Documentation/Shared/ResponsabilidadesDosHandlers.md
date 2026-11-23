# Responsabilidades dos handlers

Cada Handler concreto atende um request e possui um objetivo principal. Verificações
e efeitos necessários a esse objetivo pertencem ao mesmo caso de uso. As pastas
agrupam o contexto; não se cria uma classe para reunir as operações do contexto.

## Alterações das etapas 4 a 6

- As operações de ProductCatalogWriter e ProductVariantWriter passaram para os
  handlers de criação, alteração, ativação, variantes, categoria e exclusão.
  Cada handler coordena sua transação e seus commits.
- ProductDefaultVariantWriter sincroniza o SKU padrão e valida as variantes após
  mudanças do produto. Não recebe requests, não carrega o produto e não confirma
  transações. É compartilhado por sete handlers.
- Identidade do lote e esquema de opções usam políticas específicas. Os cinco
  Writers de estoque, reservas, cupons e operações financeiras continuam executando
  fragmentos compartilhados, sem controlar os casos de uso.
- Treze handlers singulares de CRUD deixaram de encaminhar a operação para um
  handler de lote. Os bases existentes dos lotes foram preservados.
- A prévia e a confirmação de reativação do DROP têm handlers separados; a seleção
  permanece compartilhada e é calculada sob o mesmo lock.
- Processar e entregar o pedido Store possuem Commands e handlers próprios. As duas
  rotas HTTP existentes continuam com o mesmo formato.
- Controllers deixaram de consultar DbContexts ou repositórios diretamente. Leituras
  de integração, vitrine e mídia entram por Queries, com projeções nomeadas. Os
  contratos de loja, produto, funcionário e cliente usados pelos dois módulos
  pertencem a Shared.
- OAuth e notificações do Mercado Pago possuem handlers próprios para início,
  conclusão, sandbox, recebimento, processamento e reconciliação. A infraestrutura
  mantém HTTP, SQL, criptografia, proteção de tokens e inserções condicionais.
- Workers do DROP e do Mercado Pago acionam Commands/Queries. Workers preservam
  agendamento, descoberta de pendências, escopos e recuperação por item.

## Composições preservadas

O MediatR não é proibido dentro de um handler. A composição deve representar etapas
independentes necessárias ao objetivo, sem esconder a implementação principal.
Na auditoria desta alteração foram revisados 27 handlers com 40 pontos de despacho:

| Grupo | Motivo da composição |
| --- | --- |
| Fila, reserva e pagamento DROP | Expirar compras antes de admitir/reservar/aplicar pagamento; consultar a situação resultante |
| Checkout Store e DROP | Gravar a intenção antes de processar/reconciliar o pagamento; recuperar resultados desconhecidos |
| Notificações e pagamentos Mercado Pago | Verificar o pagamento remoto e encaminhar ao caso de uso Store ou DROP correspondente |
| Assinatura SaaS | Validar equipe, reconciliar cobrança, aplicar direitos e tratar expiração sem reunir esses casos em um Service |
| Contratos e equipe | Reutilizar a validação explícita de seleção de membros |
| Aparência | Resolver publicação, migrar e permitir retentativa administrativa sem alterar locks ou idempotência |
| Consultas de cobrança | Aplicar autorização do dono e locks antes da projeção compartilhada |

Nenhuma implementação concreta de handler é injetada em outro handler. As transações
e as etapas posteriores ao commit não foram fundidas. Eliminar esses despachos apenas
para zerar a contagem mudaria os limites dos fluxos ou duplicaria operações completas.

## Adapters técnicos e limites

IProviderConnectionCredentials representa proteção, armazenamento e renovação de
credenciais. O adapter existente ProviderConnectionService continua em Integrations.
Ele não substitui os handlers de OAuth ou de pagamento.

O uso de JwtSessionService na fronteira HTTP é técnico: valida e emite tokens de
sessão. O teste arquitetural permite explicitamente esse adapter.

Foram mantidos os guards de persistência dos DbContexts, os bases de CRUD/Identity,
as entidades, as migrations e os contratos públicos. Esta revisão não redistribui
as proteções históricas dos DbContexts nem altera regras comerciais.

## Verificação

ArchitectureCompletionTests verifica um request por handler concreto, ausência de
dependência de controllers em contexto/repositório/Service de negócio, limites dos
Writers e ausência dos orquestradores removidos.

MercadoPagoWorkflowOwnershipTests usa PostgreSQL local descartável e um cliente
OAuth fictício. Exercita navegador correto, uso único do state, callbacks concorrentes,
revogação de acesso durante a rede, conta divergente, identificação do provedor e
deduplicação de notificações assinadas. Nenhuma cobrança ou requisição OAuth real é
executada. As regressões DROP também verificam que as novas consultas de reservas
e pedidos preservam o isolamento entre compradores.

A suíte completa é executada pelo roteiro
[Validação comercial para o TCC](ValidacaoComercialTcc.md).
