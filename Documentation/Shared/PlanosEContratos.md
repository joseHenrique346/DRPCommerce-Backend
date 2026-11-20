# Planos e contratos por loja — Partes 1 e 2

A Parte 1 entrega o catálogo comercial, o contrato efetivo por loja, a revisão de direitos,
a leitura do consumo local e a solicitação de mudança. Não cria cobranças, recorrência,
checkout de assinatura nem concessão de plano pelo navegador.
A Parte 2 aplica cotas ao catálogo Store, bloqueia excedentes por contrato,
oferece reativação explícita e registra atividades comerciais da loja.

A base foi portada de `feat/90-EstudoDeValorUsuarioSeparacaoContrato` para a branch de integração.
Não houve merge completo da branch antiga: os fluxos atuais de pagamento foram preservados,
e a rota antiga de alteração direta do plano não foi incorporada.

## Catálogo e identificadores

Versão inicial: `catalogVersion: 1`. Moeda: `BRL`.
Valores abaixo são mensais; a oferta anual e a política de renovação ficam para a Parte 5.

| ID | Código | Nome | Mensal | Produtos Store | Produtos DROP | Equipe adicional | Temas Store / DROP |
| ---: | --- | --- | ---: | ---: | ---: | ---: | --- |
| 1 | free | Gratuito | R$ 0 | 50 | 0 | 0 | 1 / 0 |
| 4 | initial | Inicial | R$ 49 | 100 | 0 | 1 | 3 / 0 |
| 2 | pro | Pro | R$ 99 | 500 | 0 | 3 | 10 / 0 |
| 3 | drpcommerce | DRPCommerce | R$ 199 | 2.000 | 100 | 5 | 20 / 10 |
| 5 | drpdrop | DRPDrop | R$ 149 | 0 | 100 | 5 | 0 / 10 |

IDs são estáveis, não uma ordem de superioridade. Os antigos IDs 1, 2 e 3 passam a significar
Gratuito, Pro e DRPCommerce; contratos legados mantêm sua associação.
A resposta do catálogo ordena os planos por preço.

Vitrine é a própria Loja. Gratuito e Inicial incluem Vitrine; DRPDrop não inclui catálogo
comum. Domínio próprio pertence a Pro, DRPCommerce e DRPDrop. Gratuito usa o padrão;
Inicial permite logo e escolha de tema; os superiores permitem todos os campos do tema.
Esses direitos estão representados no catálogo; sua aplicação aos fluxos de equipe,
domínio e aparência será feita nas Partes 3 e 4.

| ID | Código | Recurso | Quantidade | Mensal | Planos elegíveis |
| ---: | --- | --- | ---: | ---: | --- |
| 1 | team-small | employees | 1 | R$ 9,90 | Inicial, Pro, DRPCommerce, DRPDrop |
| 2 | team-large | employees | 5 | R$ 29,90 | Pro, DRPCommerce, DRPDrop |
| 3 | catalog-small | store_products | 100 | R$ 9,90 | Inicial, Pro |
| 4 | catalog-large | store_products | 500 | R$ 19,90 | Pro, DRPCommerce |
| 5 | launches | drop_products | 25 | R$ 14,90 | DRPCommerce, DRPDrop |

A loja possui no máximo um adicional efetivo (`Enterprise.AddOnId`).
Não há quantidade livre, combinação ou acúmulo. O adicional soma somente ao recurso
correspondente e não habilita módulos ausentes. O banco também valida a elegibilidade
do par plano/adicional por chave estrangeira composta.

## Contrato efetivo e revisão

`Enterprise` mantém `PlanId`, `AddOnId`, `ContractRevision` e `CatalogVersion`.
Toda nova loja começa em Gratuito, sem adicional, revisão 1.

`EnterpriseEntitlements.Resolve` centraliza capacidades e limites.
IDs desconhecidos e adicionais incompatíveis são rejeitados; não há conversão silenciosa
para um plano padrão. O dono principal não consome vaga de equipe.

Mudar somente o adicional incrementa a revisão, mesmo mantendo o plano.
Reaplicar exatamente o mesmo contrato não altera a revisão.
A revisão também é um token de concorrência no EF.

Uma alteração efetiva exige `EnterpriseContractWriteScope` da própria loja.
O guard de persistência é executado antes dos demais escopos: abrir apenas um fluxo
de identidade não permite conceder plano pago ou alterar limites.
Não existe endpoint público ou interno de concessão nesta etapa. A integração financeira
futura deverá revalidar dono, oferta, revisão e confirmação de pagamento antes de conceder
direitos e aplicar os efeitos nos outros módulos.

O catálogo é somente leitura na aplicação; alterações pertencem às migrations da plataforma.

## Endpoints

Rotas abaixo são do serviço Store. No Gateway atual, acrescentar `/api/store`:
por exemplo, `/api/store/api/enterprise-plans`.

As rotas de catálogo e contrato usam DTO direto, sem envelope `Result<T>`.
As rotas de produto e histórico usam o envelope habitual, com os dados em `content`.
Erros de aplicação usam ProblemDetails, com os campos adicionais `code` e `traceId` na raiz JSON.

| Método | Rota | Acesso | Resultado |
| --- | --- | --- | --- |
| GET | /api/enterprise-plans | Público | Versão, moeda, cinco planos, cinco adicionais e elegibilidade |
| GET | /api/enterprises/current/contract | Usuário com vínculo ativo na loja selecionada | Contrato efetivo, revisão e direitos calculados |
| GET | /api/enterprises/current/contract/usage | Dono ou permissão settings.manage | Consumo local e comparação informativa com os limites |
| POST | /api/enterprises/current/contract/requests | Dono principal atual | 202: intenção persistida, sem cobrança ou concessão |
| GET | /api/enterprises/current/contract/requests/{id} | Dono principal da própria loja | Situação e preços congelados da solicitação |

Nenhuma rota recebe um tenant como autoridade no body. A loja vem do vínculo autenticado.
A rota legada `PUT /api/enterprises/current/plan` não está exposta.

### Solicitar mudança

Primeiro consultar o catálogo e o contrato atual. Exemplo de escolha de Pro + Equipe ampliada:

```json
{
  "idempotencyKey": "contract-choice-unique-key",
  "planId": 2,
  "addOnId": 2,
  "expectedContractRevision": 1,
  "catalogVersion": 1
}
```

Não enviar preço, quantidade extra, aprovação ou identificador da loja.
O servidor obtém valores do catálogo. Nesse exemplo, os snapshots são R$ 99 + R$ 29,90.

A resposta 202 contém `id`, `enterpriseId`, `status`, `planId`, `addOnId`,
`expectedContractRevision`, `catalogVersion`, `currency`, `planMonthlyPrice`,
`addOnMonthlyPrice`, `totalMonthlyPrice`, `requestedAt`, `supersededAt`
e `grantsAccess: false`. O header Location aponta para a consulta da solicitação.

- Repetir chave e conteúdo retorna a mesma solicitação.
- Reutilizar a chave para outro conteúdo retorna 409 `Contract.IdempotencyConflict`.
- Uma nova escolha substitui a intenção pendente anterior, que passa a `superseded`.
- Repetir a chave de uma intenção substituída retorna seu estado original atualizado;
  não a torna pendente novamente.
- Revisão ou catálogo desatualizados retornam 409.
- Oferta desconhecida ou adicional incompatível retorna 400.
- Apenas uma intenção por loja pode estar pendente. Lock transacional da loja e índice
  único protegem requisições simultâneas.
- Uma solicitação, inclusive de Gratuito, não altera o contrato efetivo.
  Efeitos de downgrade serão implementados antes da concessão financeira futura.

Preços congelados representam a escolha, não uma fatura paga ou uma vigência contratada.

## Consumo

A leitura retorna:

- `totalStoreProducts`: principais não excluídos, incluindo inativos/arquivados;
- `activeStoreProducts` e `inactiveStoreProducts`: estado atual de Product.IsActive;
- `blockedStoreProducts`: produtos não excluídos bloqueados por contrato;
- `publishedStoreProducts`: ativos não excluídos e não bloqueados;
- `activeAdditionalMembers`: vínculos ativos e usuários ativos, incluindo sócios/gestores,
  excluindo o dono principal;
- `pendingInvitations`: convites não aceitos, não revogados e ainda válidos;
- saldo e excedente de produtos e equipe, associados à revisão consultada.

Variações não são contadas como novos produtos.
Com limite 50, dez ativos e 39 inativos/arquivados consomem 49 e deixam uma vaga.
O consumo não exige excluir todos os arquivados.

O DROP mantém outro banco. Nesta etapa, `dropProducts: null` e
`dropUsageAvailable: false` indicam ausência de integração; não significam consumo zero.
A leitura não reserva vagas e não substitui a validação transacional de escrita.
Uma mudança concorrente pode tornar a consulta informativa desatualizada; a confirmação
revalida a revisão. A Parte 2 aplica a cota de produtos sob lock da própria loja.

## Produtos, downgrade e reativação — Parte 2

A criação conta todos os principais não excluídos, inclusive arquivados, inativos e
bloqueados. SKUs/variações não ocupam outra vaga. O lote inteiro é recusado se ultrapassar
a cota; requisições simultâneas disputam a capacidade sob lock transacional de Enterprise.
Arquivar abre uma posição pública, mas não libera capacidade para criar outro cadastro.

Quando um fluxo confiável aplica plano/adicional, a mesma transação conserva publicados
os principais ativos mais antigos que couberem, ordenados por `CreatedAt` e depois `Id`.
O excedente recebe `IsContractBlocked`, `ContractBlockReason` e `ContractBlockedAt`.
Isso não altera `IsActive`, saldos, pedidos, reservas existentes ou a data de criação.
Perder o módulo Store bloqueia também os arquivados, com motivo `store_module`;
reduzir apenas a cota usa `product_limit`.

Upgrade ou retorno do adicional não republica produtos ativos bloqueados.
O usuário confirma reativação por antiguidade ou seleção explícita, dentro das posições
públicas disponíveis. Não é criação e não desarquiva produtos. Ao recuperar o módulo,
arquivados bloqueados por `store_module` voltam a ser editáveis, permanecendo arquivados.
Excedentes de cadastro continuam no painel e impedem novas criações até caber no total.

Produto bloqueado admite consulta administrativa, exclusão lógica e reativação própria.
Edição, arquivamento, metadados dos SKUs, ajuste manual de estoque e novas reservas são
recusados. O bloqueio também vale na persistência, incluindo tentativas de remover a flag
fora do escopo de reativação. A antiguidade não pode ser alterada por atualização.

A exclusão de produto bloqueado pode preservar saldo disponível, conforme decisão do
usuário. É recusada se houver quantidade reservada ou uma reserva ainda ativa vinculada.
Produto e SKUs são excluídos logicamente, com saldo, movimentos e histórico preservados.
Para produtos sem bloqueio, continua a exigência anterior de zerar saldo e reservas.

Catálogo público, busca, slug e referências de produtos em aparência publicada omitem
produtos bloqueados. O carrinho antigo mantém o item removível, com nome genérico e sem
slug, SKU, imagens, opções ou preço; indica indisponibilidade e impede sua compra.
Checkout e criação de itens/reservas revalidam a disponibilidade antes de gravar.
Pedidos já criados continuam usando seus snapshots e podem receber pagamento, baixa
ou liberação das reservas existentes. Não são reprecificados nem excluídos.

### Endpoints da Parte 2

| Método | Rota | Acesso | Uso |
| --- | --- | --- | --- |
| POST | /api/enterprises/current/contract/product-impact | Dono principal | Prévia do efeito de uma oferta, sem alterar o contrato |
| POST | /api/products/contract-reactivation/preview | products.edit | Prévia por antiguidade ou seleção, sem reservar posições |
| POST | /api/products/contract-reactivation | products.edit | Confirma a reativação dentro da capacidade atual |
| GET | /api/store-activity | store.history.view | Histórico paginado da própria loja |

Prévia do contrato recebe `planId`, `addOnId` opcional, `expectedContractRevision` e
`catalogVersion`. Retorna totais, limite, publicados antes/depois, quantidade a bloquear,
excedente total e amostra dos primeiros 100 afetados, com indicador de truncamento.
Não concede plano nem executa cobrança.

Exemplo de reativação:

```json
{
  "expectedContractRevision": 4,
  "mode": "selected",
  "productIds": [101, 102]
}
```

Para antiguidade, usar `mode: "oldest"` e omitir `productIds`.
Seleção manual aceita até 5.000 IDs únicos positivos. Produto de outra loja, excluído,
arquivado ou já reativado retorna 404; uma seleção maior que a capacidade retorna 409,
sem reativar parcialmente. Revisão antiga retorna 409. Token administrativo invalidado
por mudança de contrato precisa ser renovado antes da chamada.

Principais códigos: `ProductContract.CreationLimit`, `ProductContract.PublicationLimit`,
`ProductContract.Blocked`, `ProductContract.Unavailable`, `Contract.RevisionChanged`
e `Inventory.StockInUse`.

### Histórico comercial

`atividade_loja` registra criação, edição, publicação, arquivamento, bloqueio, reativação,
exclusão e restauração de produtos; alterações de SKUs; e mudanças efetivas do contrato.
Cada evento traz horário UTC, motivo, correlação, autor quando disponível e snapshots
de nome/SKU. Detalhes registram os campos alterados ou a mudança de contrato, sem CPF,
IP, credenciais ou dados de pagamento do comprador. Alterações técnicas de saldo
continuam no livro de movimentos de estoque.

O servidor acrescenta os eventos na mesma transação da alteração; a aplicação recusa
inserção direta, edição e exclusão desses registros. A exclusão lógica do produto
preserva o histórico. A referência de autor aceita nulo se o usuário for removido;
o tipo original de autor e os snapshots permanecem. Não há expiração automática.
A permissão própria `store.history.view` pode ser atribuída sem conceder edição de produtos.

Filtros: `productId`, `eventType`, `fromUtc`, `toUtc`, `page` (padrão 1) e `pageSize`
(padrão 30, máximo 100). Datas precisam ser UTC com `Z`. Ordenação por ID decrescente.
A resposta mantém isolamento pela loja autenticada e não recebe tenant como autoridade.

A aparência pública filtra referências na leitura, sem reescrever rascunhos ou revisões.
IDs indisponíveis saem de listas; referências escalares ficam `null`.
O checksum/ETag acompanha a projeção atual, inclusive nas consultas por revisão.
Os manifests Store precisam ser revalidados (`must-revalidate`), pois a disponibilidade
dos produtos pode mudar sem criar outra revisão visual.

## JWT e implantação

Tokens administrativos com loja selecionada recebem `plan_id`, `contract_revision`
e `catalog_version`. A validação consulta o contrato atual.
Mudar plano ou adicional invalida o token antigo daquela loja; renovar a sessão ou
selecionar novamente a loja emite claims atuais. Outras lojas do mesmo usuário não
são revogadas por essa alteração.

Após implantar, tokens antigos com loja selecionada, sem as novas claims, precisam
ser renovados. Tokens globais sem loja e tokens de cliente mantêm os contratos anteriores.

Migrations:

1. `20261103021224_EnterprisePlans`: base compatível com a migration já existente
   na branch antiga. Cria catálogo legado e PlanId quando ainda não foram aplicados.
2. `20261119203614_EnterpriseCommercialContracts`: evolui os três planos,
   acrescenta Inicial/DRPDrop, adicionais, elegibilidade, revisão e solicitações.
3. `20261119235550_AddProductContractLimitsAndActivity`: bloqueio comercial de produtos,
   histórico e permissão de consulta. Reconcilia o catálogo existente pelos limites,
   preserva dados e registra os bloqueios iniciais com autor `system`.

A migração direta não apaga lojas ou pedidos e não executa cobranças.
Revisar SQL e backup antes de aplicar em um ambiente com dados.
O Down remove tabelas novas e não é uma forma de preservar solicitações; contratos
nos novos IDs também exigem tratamento antes de voltar ao catálogo legado.

Nesta implementação, migrations foram aplicadas somente em banco descartável de testes.
Não foram aplicadas aos bancos de desenvolvimento existentes nem a produção.

## Verificação

Projeto: `DROP-Ecommerce/tests/StoreCommerce.Contracts.Tests`.

```powershell
dotnet test DROP-Ecommerce/tests/StoreCommerce.Contracts.Tests/StoreCommerce.Contracts.Tests.csproj -p:InstallGitHooks=false
```

Testes HTTP/PostgreSQL precisam de `DRP_CONTRACTS_TEST_POSTGRES`, configurada privadamente
com uma conexão local que possa criar banco. Sem ela, os testes de integração são
explicitamente ignorados; os testes do catálogo continuam executando.

A fixture cria `drp_contract_checks_<id>`, aplica migrations, testa a evolução de
contratos legados, inicia a API em TestServer e remove somente esse banco no fim.
A conexão informada serve para acesso ao servidor; seu banco de origem não recebe migrations.
Mercado Pago fica desabilitado e os workers Store são removidos da fixture.

Em máquina com SDK 10 e sem runtime 9, definir `DOTNET_ROLL_FORWARD=Major` para os comandos
da API/EF; o projeto de testes também permite roll-forward.

Cobertura: catálogo e preços, elegibilidade, snapshots, revisão, autenticação,
permissões, isolamento, concorrência de solicitações, concessão proibida por identidade
e preservação de outras lojas do mesmo usuário. A Parte 2 cobre migração de catálogo
excedente, total com arquivados, atomicidade de lote, disputa pela última vaga, downgrade,
reativação, adicional, edição/exclusão bloqueadas, estoque preservado, histórico e
permissão própria, projeção de aparência, catálogo público, carrinho e checkout.
Pedido criado antes do bloqueio é pago pelo gateway manual local e mantém snapshots;
reservas anteriores ainda podem ser efetivadas. Na verificação local, 56 testes passaram.

## Próximas partes

Produtos e histórico Store estão protegidos na Parte 2. Equipe/domínio ficam para
a Parte 3; cotas de temas, personalização e mídia para a Parte 4; cobrança SaaS para
a Parte 5; fluxo completo e consumo DROP para a Parte 6. A interface ainda precisa
consumir as prévias, explicar bloqueios e oferecer seleção/reativação e histórico.
Não se deve anunciar os limites de todos os módulos como proteção já concluída.
