# Planos e contratos por loja — Partes 1 a 4

A Parte 1 entrega o catálogo comercial, o contrato efetivo por loja, a revisão de direitos,
a leitura do consumo local e a solicitação de mudança. Não cria cobranças, recorrência,
checkout de assinatura nem concessão de plano pelo navegador.
A Parte 2 aplica cotas ao catálogo Store, bloqueia excedentes por contrato,
oferece reativação explícita e registra atividades comerciais da loja.
A Parte 3 aplica vagas de equipe, escolha no downgrade, restauração de acessos bloqueados
por contrato e proteção do uso de domínio próprio.
A Parte 4 aplica catálogo de temas e personalização por plano, fallback público e logos
armazenados no Store, compartilhados com o DROP por API autenticada.
A Parte 5 acrescenta assinatura mensal automática, faturas, confirmação financeira,
trocas de oferta e carência. A homologação externa dessa cobrança permanece pendente.

A base foi portada de `feat/90-EstudoDeValorUsuarioSeparacaoContrato` para a branch de integração.
Não houve merge completo da branch antiga: os fluxos atuais de pagamento foram preservados,
e a rota antiga de alteração direta do plano não foi incorporada.

## Catálogo e identificadores

Versão inicial: `catalogVersion: 1`. Moeda: `BRL`.
Valores abaixo são mensais. A Parte 5 implementa renovação automática mensal; oferta anual fica adiada.

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
Equipe e domínio são aplicados na Parte 3. Cotas de temas, personalização e logos
são aplicados na Parte 4, conforme os modelos efetivamente cadastrados pela plataforma.

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
- `blockedAdditionalMembers`: vínculos bloqueados pelo contrato, separados dos desativados pelo dono;
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

## Equipe e domínio — Parte 3

A cota pertence à loja e conta todos os vínculos administrativos adicionais ativos,
incluindo sócios/gestores, com User global ativo. O dono principal não consome vaga.
Não se limita à tabela Employee. Bloqueio comercial preserva `IsActive` e possui flag
e data próprias no vínculo; não desativa o User nem os vínculos em outras lojas.

Convites ainda não aceitos/revogados e não expirados ocupam posições disponíveis.
Convite e ativação são validados sob o mesmo lock transacional de Enterprise.
Reenviar convite ao mesmo e-mail substitui o anterior, sem consumir duas posições.
Revogar ou expirar devolve a posição. Aceitar converte o convite em acesso na mesma
transação, sem dupla contagem, e revalida a capacidade efetiva do contrato.
Convites anteriores à redução permanecem registrados, mas não podem ser aceitos
enquanto não houver vaga. Não há promessa de que um convite antigo concederá acesso.

### Escolha no downgrade

Novo endpoint `POST /api/enterprises/current/contract/access-impact`, restrito ao dono,
recebe oferta, revisão e versão do catálogo. Retorna limite de equipe, contagens,
IDs elegíveis, necessidade de escolha, IDs que serão bloqueados e domínios afetados.
Sem seleção, informa a necessidade; não escolhe pessoas automaticamente.
A prévia não altera vínculos nem concede plano.

Quando o alvo não comporta a equipe ativa, `POST .../contract/requests` exige
`retainedMembershipIds`. O dono informa os vínculos que permanecerão dentro da cota:

```json
{
  "idempotencyKey": "downgrade-choice-unique",
  "planId": 4,
  "expectedContractRevision": 2,
  "catalogVersion": 1,
  "retainedMembershipIds": [123]
}
```

Para Gratuito com equipe ativa, enviar explicitamente `[]`; omitir a lista não expressa
uma escolha. IDs precisam ser positivos, únicos, da própria loja e de vínculos ativos,
não bloqueados, com usuário ativo, excluindo o dono. A lista não pode superar a cota alvo.
A solicitação guarda a seleção e a inclui na comparação de idempotência; mesmo conjunto
em outra ordem equivale à mesma seleção. Mudar o conjunto com a mesma chave retorna 409.

O futuro fluxo financeiro precisa recuperar essa seleção persistida e passá-la ao
escopo confiável de concessão. A aplicação revalida os vínculos e a capacidade na mesma
transação do contrato, dos produtos e dos domínios. Redução sem escolha é recusada com
`Team.SelectionRequired`; a solicitação continua sendo somente uma intenção.

Recuperar capacidade por upgrade/adicional restaura automaticamente vínculos bloqueados
exclusivamente pelo plano, em ordem de bloqueio, entrada e ID, dentro das posições
livres após os convites válidos. Vínculos desativados pelo dono ou de usuários inativos
não são restaurados. Desativar manualmente um bloqueado limpa seu motivo comercial,
para impedir reabertura por upgrade. Ativação manual também revalida a cota.

As sessões, refresh e escolhas de loja omitem/recusam bloqueados. O bloqueio incrementa
`AccessVersion`; não é possível reaproveitar token antigo. A revisão do contrato continua
invalidando os tokens selecionados da própria loja. Uma transferência de dono considera
a troca como uma única vaga durante suas duas gravações; não cria capacidade extra.

Dados legados acima do limite, ainda sem flags comerciais, não autorizam sessões de
equipe acima da cota. O dono mantém acesso para ajustar os vínculos. A migration não
escolhe quem permanece nem desativa usuários. Para redução automática por vencimento,
a Parte 5 aplica sete dias de carência e retorno ao Gratuito, com bloqueio de toda a
equipe adicional e preservação do dono.

### Domínio próprio e fallback

Gratuito e Inicial mantêm o subdomínio da plataforma. Sem direito a domínio próprio,
cadastro, verificação e ativação de host externo são recusados com `Domain.PlanRequired`.
A verificação recusa o plano antes de consultar DNS e revalida a escrita após o lock.

Perder esse direito desativa os domínios externos e marca bloqueio comercial, preservando
host, cadastro e verificação. O subdomínio verificado da plataforma volta a ser o principal;
se o cadastro legado não tiver um, a aplicação cria um fallback exclusivo a partir da loja
e do domínio-base configurado. A troca respeita o índice de domínio primário na mesma
transação. A administração continua acessível pelo backoffice.

Upgrade não ativa domínio externo automaticamente. O dono pode reativá-lo pelo endpoint
existente `PUT /api/enterprise-domains/{id}/publication`, com direito vigente e verificação
preservada. O domínio bloqueado continua visível na listagem administrativa.

`IEnterpriseDomainAccess` centraliza a resolução por host: domínio ativo/verificado,
loja ativa e direito vigente. É usado pelo StoreTenantResolver, validação da sessão de
comprador e `GET /api/internal/tenants/resolve`, consumido pelo DROP com autenticação de
serviço. A verificação do contrato impede uso de host externo mesmo com dados legados
que ainda indiquem domínio ativo sem flag de bloqueio.

### Respostas e histórico

Listagens administrativas de vínculos incluem `isContractBlocked` e `contractBlockedAt`;
domínios incluem `isContractBlocked`. Consumo de equipe usa os acessos efetivamente
disponíveis; `remainingEmployeeSlots` desconta também convites pendentes.

Bloqueio/restauração, gestão de vínculo, convite e domínio geram atividades da loja
na mesma transação. Detalhes guardam tipo e IDs quando já disponíveis, sem e-mail de
convidado, token de convite, senha, desafio DNS ou host no histórico comercial.
Eventos de criação usam a correlação da operação quando o ID ainda não foi gerado.
A consulta continua restrita à permissão própria `store.history.view`.

Códigos principais: `Team.MemberLimit`, `Team.InvitationLimit`, `Team.SelectionRequired`,
`Team.InvalidSelection`, `Identity.MembershipUnavailable` e `Domain.PlanRequired`.

## Temas, personalização e logos — Parte 4

O catálogo visual usa `catalogPosition` estável por chave/canal. Versões da mesma chave
mantêm a posição; o modelo padrão ocupa a posição 1. Gratuito/Inicial/Pro/DRPCommerce
podem usar as posições Store 1/3/10/20, respectivamente. DRPCommerce e DRPDrop têm dez
modelos no canal DROP. Os limites contam modelos, sem multiplicar pelas versões.

A plataforma cadastra os contratos e seus renderers; um banco sem catálogo continua sem
temas utilizáveis. Esta etapa não inventa modelos visuais nem instala renderers no frontend.
O endpoint interno aceita a posição explícita ou atribui a próxima a uma chave nova;
reordenar uma chave ou reutilizar a posição de outro modelo é recusado.

No schema, `x-plan: business` indica dados comerciais necessários também ao Gratuito;
`x-plan: logo` identifica uma referência de ativo para Inicial e superiores; os demais
campos são personalização completa. `brand.name` e `brand.logoAssetId` mantêm compatibilidade.
Gratuito usa defaults e dados comerciais; Inicial escolhe entre seus modelos e troca o
logo, preservando os demais defaults; superiores editam todos os campos permitidos pelo schema.

Escolha, início de edição, gravação, prévia, publicação, rollback e persistência revalidam
o contrato atual. Uma edição aberta antes de um downgrade não conserva o direito antigo.
No Store, contrato e aparência compartilham o lock da loja e a gravação revalida o dono.
O DROP consulta o Store por API interna e nega acesso quando não consegue confirmar direitos.

No downgrade, o público usa a última revisão elegível daquele alvo/origem, até o número
da revisão efetiva; na ausência dela, os defaults do modelo padrão permitido. A busca
não se limita às últimas 100 revisões. Campos fora do plano são projetados para defaults
na leitura. Configurações e ponteiros salvos continuam preservados no painel; ao recuperar
direitos, a publicação preservada pode se tornar elegível novamente.

URLs por revisão fora do plano retornam 404; modelos ainda permitidos têm campos e
ETag recalculados conforme os direitos atuais. Store e DROP exigem revalidação de cache,
inclusive por revisão. A fixação visual de evento DROP é preservada, sujeita aos direitos
comerciais atuais. Esta etapa não altera prazos de reservas nem conclui o fluxo comercial DROP.

Logos são ativos por loja no PostgreSQL do Store, com binário imutável, hash, dimensões e
GUID público. Upload aceita PNG estático sem entrelaçamento, até 1 MiB, com dimensões e
descompressão limitadas; valida estrutura/CRC e remove metadados opcionais. Não aceita URL
arbitrária. Há limite técnico de 25 MiB de uploads ativos por loja e deduplicação do conteúdo.
Uploads sem uso podem ser removidos; referências de edição/revisões preservadas impedem
exclusão. O DROP retém o ativo antes de validá-lo para uso, evitando exclusão no Store
quando a referência existe só no outro banco. A retenção é conservadora e não tem liberação
automática nesta etapa, inclusive quando uma tentativa falha depois de reservar o ativo.

Rotas Store adicionadas:

| Método | Rota | Acesso / finalidade |
| --- | --- | --- |
| POST | /api/media-assets | Dono com direito a logo; multipart com campo file |
| GET | /api/media-assets | Dono; últimos 100 metadados da própria loja |
| GET | /api/media-assets/{id}/content | Dono; consulta privada do logo |
| DELETE | /api/media-assets/{id} | Dono; exclui upload sem uso preservado |
| GET | /api/storefront/media/{publicId} | Host da loja, GUID público e direito vigente |
| GET | /api/internal/media-assets/{id} | Referência para serviço autenticado |
| POST | /api/internal/media-assets/{id}/retain-for-drop | Retenção idempotente para o DROP |
| GET | /api/internal/enterprises/{id}/appearance-rights/{channel} | Direitos atuais para serviço autenticado da mesma loja |

As rotas existentes de aparência continuam valendo. `GetStatus` acrescenta `capabilities`;
o catálogo acrescenta `catalogPosition`; manifests e prévias incluem a URL do logo em
`assetBindings.primaryLogo`. URLs de mídia são relativas à API Store: compor `/api/store`
quando o frontend utiliza o Gateway. A URL por GUID destina-se à exibição pública do logo;
documentos privados não pertencem a esse armazenamento.

Detalhes, exemplos e limites estão em [Aparência](Appearance.md). O frontend ainda precisa
entregar os renderers/catálogo visual e consumir direitos, campos permitidos, upload e fallback.

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
4. `20261120010718_AddTeamAndDomainContractLimits`: flags de bloqueio de vínculo/domínio,
   data do bloqueio do vínculo e seleção de equipe persistida nas solicitações.
   Não altera usuários globais nem seleciona automaticamente uma equipe legada.
5. Store `20261120025127_AddAppearanceEntitlementsAndMedia`: posição estável de temas e
   tabela de logos. Preserva conteúdo/versões legadas e atribui posição pela primeira versão cadastrada.
6. Store `20261120030208_RetainLogosReferencedByDrop`: retenção de logos consultados para uso no DROP.
7. DROP `20261120025225_AddAppearanceThemeEntitlements`: posição de temas com a mesma preservação do legado.

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
reservas anteriores ainda podem ser efetivadas. A base das Partes 1 e 2 possui 56 testes.
A Parte 3 acrescenta cenários de vagas, convites concorrentes, expiração/aceitação,
escolha e idempotência, restauração, isolamento de outras lojas, transferência de dono,
fallback de domínio e validação pública/interna/de sessão.
A Parte 4 acrescenta testes de modelos e versões, campos por plano, downgrade/ETag,
histórico longo, concorrência, migração legada, PNG/upload/exclusão, isolamento de mídia
e handlers DROP com PostgreSQL próprio e chamadas HTTP reais ao Store em TestServer.
Na validação das Partes 1 a 4, 102 testes passaram e a solução compilou sem erros.

## Parte 5 — assinatura mensal automática da plataforma

A Parte 4 está no commit local `cb397f5`, sem push. A Parte 5 permanece sem commit.
Em 20/11/2026 foram definidas renovação mensal automática, upgrade imediato,
downgrade no próximo vencimento e sete dias de carência. E-mails ficam para depois;
a consulta de cobrança fornece os avisos persistentes para o painel.

### Ciclo e mudanças

- A adesão usa Assinaturas sem plano associado (`POST /preapproval`, `status: pending`),
  com autorização hospedada no Mercado Pago, sem receber dados de cartão no backend.
  A recorrência é de um mês, pelo total do plano e do único adicional. Não há oferta anual.
- `authorized`, link de retorno e dados do navegador não comprovam pagamento. A liberação
  exige fatura recorrente associada à assinatura e pagamento consultado pela API, aprovado,
  com recebedor, moeda e valor corretos, sem reembolso. Uma cobrança de verificação do
  meio de pagamento, isoladamente, não concede plano.
- O período vem da data de débito da fatura. A âncora mensal original é preservada,
  inclusive em meses curtos: 31/jan → 28/fev → 31/mar. A confirmação tardia não cria um
  novo período de trinta dias a partir do recebimento do webhook.
- Aumento do total mensal é upgrade imediato: calcula no servidor a diferença entre os
  totais, proporcional ao tempo restante do período pago, arredondada para centavos.
  Usa Checkout Pro somente para essa diferença. Após aprovação, atualiza o valor das
  próximas cobranças da assinatura e concede os direitos, mantendo o vencimento.
- Trocas com total menor ou igual entram no próximo vencimento. O valor da próxima
  recorrência é sincronizado antes; os direitos atuais permanecem até a data prevista.
  A escolha de equipe é validada ao solicitar e ao aplicar. Pode ser atualizada enquanto
  a mudança está pendente, caso os vínculos tenham mudado.
- Downgrade ao Gratuito cancela as cobranças futuras e mantém o período já pago;
  na data escolhida aplica o Gratuito, sem acrescentar carência a um cancelamento voluntário.
  Uma autorização inicial ainda não paga pode ser cancelada pela escolha do Gratuito,
  antes de contratar outra oferta.
- Sem renovação paga, o prazo de carência é `paidUntil + 7 dias`, em UTC. Os direitos
  permanecem durante essa janela. Tentativas rejeitadas, retries e indisponibilidade do
  provedor não mudam a data. Ao terminar, aplica Gratuito com seleção vazia de equipe
  adicional, preservando dono, dados, vendas e histórico e acionando as proteções das Partes 2–4.
- Pagamento recuperado após a carência pode restaurar os direitos no período mensal
  correspondente; não reinicia o ciclo. Upgrade abandonado expira sem bloquear nova
  escolha. Captura tardia de um upgrade já abandonado, duplicidade, reembolso, chargeback
  ou divergência financeira exige revisão; não gera estorno automático.
- Durante a inadimplência, regularizar a mensalidade antes de trocar para outra oferta
  paga. Isso evita alterar o preço de uma fatura vencida que o provedor já está tentando cobrar.

Referências oficiais: [adesão hospedada com pagamento pendente](https://www.mercadopago.com.br/developers/pt/docs/subscriptions/integration-configuration/subscription-no-associated-plan/pending-payments),
[gerenciamento de assinatura](https://www.mercadopago.com.br/developers/pt/docs/subscriptions/subscription-management),
[faturas recorrentes](https://www.mercadopago.com.br/developers/pt/reference/online-payments/subscriptions/authorized-payment-search/get).
O proporcional de upgrade acima é uma regra da plataforma; não utiliza o pro rata de
adesão do Mercado Pago como se fosse uma troca de plano.

### Endpoints

As rotas de gestão exigem usuário administrativo e dono principal atual da loja selecionada.
A validação relê vínculo, usuário e revisão; não aceita tenant, preço ou condição de pago no body.

| Método | Rota | Finalidade |
| --- | --- | --- |
| GET | `/api/enterprises/current/billing` | Situação, mensalidade, vencimento, carência, checkout e avisos para o painel |
| GET | `/api/enterprises/current/billing/invoices?beforeId=...` | Até 50 faturas da loja, com valores e períodos congelados; paginação por ID |
| POST | `/api/enterprises/current/billing/checkout` | Recebe `contractChangeId` de uma solicitação existente; inicia adesão ou mudança, de forma idempotente |
| POST | `/api/enterprises/current/billing/reconcile` | Consulta o provedor e atualiza o andamento; não força débito nem aceita status declarado pelo cliente |
| PUT | `/api/enterprises/current/billing/pending-team` | Recebe `expectedContractRevision` e `retainedMembershipIds` para corrigir a equipe de um downgrade pendente |
| POST | `/api/platform-billing/mercado-pago/webhook` | Recebe notificações assinadas de `payment`, `subscription_preapproval` e `subscription_authorized_payment` |

No gateway, as rotas Store continuam precedidas por `/api/store`. O fluxo do frontend é:
catálogo → prévias de impacto → solicitação de contrato → checkout → autorização/pagamento
hospedado → consulta do andamento. Depois da concessão, renovar o token da loja, pois a
revisão do contrato invalida o token selecionado antigo. Não há endpoint público de concessão.
O e-mail da conta do dono é usado como `payer_email`; a conta usada na autorização precisa
ser compatível com esse e-mail conforme a validação do Mercado Pago.

### Persistência e segurança

A migration `20261120150023_AddPlatformSubscriptions` cria `assinatura_plataforma`,
`fatura_plataforma` e `notificacao_cobranca_plataforma`. Nenhuma migration foi aplicada
pela implementação aos bancos existentes do usuário; os testes usam bancos descartáveis.

Há no máximo uma assinatura aberta por loja. Recebedor/ambiente, ofertas, preços,
correlação e períodos ficam preservados. Tentativas de criação são gravadas **antes**
dos POSTs externos. Se a resposta se perder, a recuperação busca por correlação e
valida o objeto encontrado; busca vazia não autoriza um novo POST. A busca de preferências
cobre os últimos 90 dias e ambiguidades precisam de revisão.

O lock transacional da loja serializa reconciliação, pagamentos e concessão, inclusive
em processos concorrentes. Os direitos, a revisão, os bloqueios de produtos/acessos/domínios
e o histórico são salvos na mesma transação da efetivação local. O workflow financeiro
possui escopo próprio, verificado antes do bypass de identidade; termos financeiros não
podem ser alterados por CRUD genérico nem mesmo abrindo apenas o escopo de identidade.
Contratos legados pagos sem assinatura são recusados no checkout e precisam de conciliação
pela plataforma; a migration não os converte em cobranças reais.

O webhook valida HMAC, timestamp, tópico e correspondência entre body e query, grava
uma inbox durável e responde sem conceder plano. Assinaturas usam IDs alfanuméricos;
o webhook de pagamentos dos lojistas conserva a validação numérica original e seu
próprio segredo. O worker concilia assinaturas e notificações, mesmo sem entrega de um
webhook. A rotina local de vencimento continua sendo executada quando as chamadas ao
provedor estão desabilitadas ou falham. O worker deve estar ativo; sua cadência é de
30 segundos e cada assinatura é consultada aproximadamente a cada minuto.

### Configuração privada e teste externo

`PlatformBilling` é independente de `MercadoPago`, das conexões OAuth dos lojistas e dos
pedidos/transações de mercadorias. A integração e a produção continuam desabilitadas por padrão.

| Chave em `PlatformBilling` | Finalidade |
| --- | --- |
| `Enabled` | Habilitação explícita; padrão `false` |
| `AllowProduction` | Liberação operacional de produção; padrão `false` |
| `Environment` | `Sandbox` ou `Production`; padrão `Sandbox` |
| `AccountId` | ID esperado da conta recebedora da plataforma |
| `AccessToken` | Credencial privada da conta da plataforma; nunca versionar |
| `WebhookSecret` | Assinatura privada da aplicação dessa cobrança |
| `PublicApiBaseUrl` | Base HTTPS pública da API, sem query string |
| `ReturnUrl` | Retorno HTTPS do backoffice; não comprova pagamento |
| `SignatureToleranceSeconds` | Tolerância da assinatura; padrão 600 segundos |

O adapter exige conta brasileira e confere a identidade da credencial em `/users/me`.
O perfil de sandbox usa vendedor de teste, sem inferir o ambiente pelo prefixo da chave
ou por `live_mode` isoladamente. Cadastrar a URL própria do webhook com os três tópicos
acima e o segredo correspondente. Não reutilizar a URL de notificações de vendas.

A implementação foi validada com HTTP simulado e PostgreSQL local descartável.
Ainda é necessário configurar credenciais/URLs, aplicar a migration no banco escolhido,
implementar a interface e validar a adesão hospedada e a recorrência com o Mercado Pago.
Não houve cobrança real, uso de credenciais reais, abertura de túnel ou teste externo
nesta etapa. Casos de revisão financeira não possuem resolução automática nem endpoint
administrativo de estorno; não marcar a homologação externa como concluída.
E-mails da plataforma ficam adiados; eventuais comunicações emitidas pelo próprio
Mercado Pago seguem o funcionamento do provedor.

Validação local: 156 testes aprovados na suíte completa, incluindo a evolução das
migrations e os fluxos de assinatura. Os cenários de cobrança usam HTTP simulado.
O ajuste final de bloqueio de troca paga durante inadimplência foi verificado novamente
nos testes de assinatura. A solução compila sem erros.

## Próximas partes

Produtos e histórico Store estão protegidos na Parte 2; equipe e domínio, na Parte 3.
Temas, personalização e logos estão protegidos no backend na Parte 4. A Parte 5 acrescenta
a cobrança SaaS automática; fluxo completo e consumo DROP continuam na Parte 6. A interface ainda precisa
consumir as prévias/direitos, explicar bloqueios, oferecer seleção/reativação e histórico,
e entregar o catálogo visual e os renderers reais.
Não se deve anunciar os limites de todos os módulos como proteção já concluída.
