# Aparência versionada — Store e Drop

O backend oferece temas fixos mantidos pela plataforma, com configurações validadas, edição isolada, preview, publicação atômica e histórico. Enterprise continua sendo a loja/tenant. A aparência usa EnterpriseId e não cria um novo tenant Store.

## Organização

As bibliotecas em `DROP-Ecommerce/Shared/SharedCommerce.*`, no grupo `4. Shared`, concentram Appearance e Security nas camadas comuns de Domain, Application, Infrastructure.Data e Api. Os contratos HTTP pertencem a Application, sem projeto separado de Contracts. Store e Drop registram adapters próprios de contexto, recursos e banco. Não existe banco ou microsserviço novo de temas. Consulte a [estrutura do Shared](README.md).

Os arquivos C# ficam em subpastas por responsabilidade:

- `SharedCommerce.Api/Controllers`: controllers-base de administração e consulta pública.
- `SharedCommerce.Application/Contracts/Appearance/Requests`, `Responses` e `Manifests`: entradas, respostas e manifesto público, com um tipo por arquivo.
- `SharedCommerce.Domain/Entity`, `StaticEntity` e `Exceptions`: entidades persistidas, classificações fixas e falhas do domínio.
- `SharedCommerce.Application/Features/Commands`, `Features/Notifications`, `Interfaces`, `Mappings` e `Validation`: handlers e resultados dos casos de uso, contratos internos, mapeamento de respostas e validação. Commands e Queries são agrupados por entidade, seguindo Store e Drop.
- `SharedCommerce.Infrastructure.Data/Configurations`, `Context`, `Repositories`, `Providers` e `Workers`: mapeamentos EF, escopo de gravação, persistência, provedores e manutenção.

Cada entidade tem seu próprio `IEntityTypeConfiguration<T>`, em uma subpasta de `Configurations`. Os dois DbContexts aplicam essas configurações e mantêm localmente os filtros e relacionamentos específicos do módulo.

Os tipos de apoio usam o prefixo `Input` para entrada e `Output` para saída. `InputAppearanceTarget` fica em `Contracts/Appearance/Requests`; `OutputThemeDescription`, em `Contracts/Appearance/Responses`; e `OutputResolvedAppearance`, junto do caso de uso `Features/Commands/Appearance/Queries/ResolvePublishedAppearance`.

As quatro entidades persistidas de aparência utilizam as bases de `SharedCommerce.Domain/Entity/Base`: `Appearance`, `AppearanceRevision`, `ThemeRelease` e `AppearanceEditSession` herdam de `BaseEntity`, com identificador `long` gerado pelo PostgreSQL. A base concentra o identificador, `CreatedAt`, `UpdatedAt` e `SetChangedDate`. A criação é registrada pelo PostgreSQL, e os dois DbContexts atualizam a data de alteração antes de salvar entidades modificadas. `BaseValidate` reúne as validações básicas de identificadores, textos e datas; os validadores de configuração/recursos e a transformação de migração continuam em `Application/Validation`. As regras de estado permanecem nas entidades. As bases compartilhadas não referenciam os domínios de Store ou Drop.

As migrations `AppearanceEntityBases` adicionam as datas às quatro tabelas existentes, sem alterar suas chaves ou relações. Nas revisões já publicadas, `CreatedAt` recebe o valor de `PublishedAt`. Para os demais registros existentes, a criação recebe a data da atualização do banco, pois não havia uma data anterior armazenada; `UpdatedAt` permanece nulo até a próxima alteração.

As migrations `AppearanceSessionLongId` convertem o identificador das sessões de edição para `bigint` com geração por identidade. Os registros recebem novos IDs numéricos, preservando configurações, estados, versões, datas e referências à aparência e às revisões. Após a conversão, o cliente deve consultar novamente a sessão atual: `sessionId` é um número `int64`, e as rotas aceitam valores `long` maiores que zero. A reversão da migration também atribui novos identificadores no formato anterior, sem recuperar os UUIDs originais.

Os handlers do Shared executam edição, validação, publicação, histórico e migração. Cada handler coordena seus próprios acessos ao repositório e transações; não existe um serviço intermediário executando esses fluxos. `AppearanceResponseMapper` apenas transforma dados em respostas e calcula a identificação do conteúdo, sem consultar o banco ou executar casos de uso. Os alvos utilizam `AppearanceTargetTypeId` e `DropEventId`; cada módulo limita os tipos que aceita. O Store usa a resolução direta de uma publicação e não implementa operações de evento.

No Drop, `ResolveDropPublishedAppearanceQueryHandler` escolhe a revisão própria, o padrão ou a revisão fixada ao evento. `AppearancePublishedNotificationHandler` atualiza essa referência na mesma transação de uma publicação/rollback; migrações mantêm a revisão de campanhas já ativas. A resolução por slug e a fixação na ativação também pertencem exclusivamente ao Drop. O repositório de eventos apenas lê os dados; as decisões ficam nos handlers do módulo. A aplicação do Drop registra seu handler para a consulta interna de resolução de publicação, e o mecanismo comum não conhece eventos, status de campanha ou regras de fallback.

O worker envia `RunAppearanceMaintenanceCommand` pelo MediatR. Seu handler consulta as migrações pendentes dos temas já cadastrados e encaminha cada alvo para `MigrateAppearanceCommandHandler`, que executa a migração. A tentativa administrativa passa antes pela autorização em `RetryAppearanceMigrationCommandHandler`.

O Store não referencia projetos do Drop. O Drop utiliza a integração por API com o Store para os recursos comerciais que consome; ambos utilizam as bibliotecas comuns e mantêm sua própria persistência.

Cada módulo persiste em seu próprio banco PostgreSQL:

| Modelo | Tabela | Responsabilidade |
|---|---|---|
| ThemeRelease | versao_tema | Chave/versão, schema JSON, defaults, checksum e plano de depreciação |
| Appearance | aparencia | Tipo de alvo, evento opcional e ponteiro para a publicação atual |
| AppearanceEditSession | sessao_edicao_aparencia | Editor, documento de trabalho, versão, expiração e preview |
| AppearanceRevision | revisao_aparencia | Publicação imutável, autoria, checksum e origem de rollback/migração |
| AppearanceEditSessionStatus | status_sessao_edicao_aparencia | Estados fixos da edição |
| AppearanceRevisionOrigin | origem_revisao_aparencia | Origens fixas das publicações |
| AppearanceChannel | canal_aparencia | Canais de aparência da plataforma |
| AppearanceTargetType | tipo_alvo_aparencia | Alvos fixos Store (1), DropDefault (2) e DropSpecific (3) |

Schemas, configurações e previews usam jsonb. Campos de busca, referências e estados ficam em colunas relacionais. FKs compostas preservam tenant e alvo das revisões/sessões. Filtros de leitura e proteção de SaveChanges impedem gravação fora dos fluxos próprios.

As classificações seguem o padrão de entidades estáticas do projeto: `BaseStaticEntity`, instâncias `static readonly`, IDs fixos, descrição e carga inicial por `HasData`. Cada uma também possui um `Code` estável para manter os valores já utilizados pelos contratos públicos. Os registros são mantidos por migrations e não podem ser alterados pelos fluxos de gravação de aparência.

`AppearanceEditSession.AppearanceEditSessionStatusId` substitui o antigo `State`; os estados são InEdit (1), InPreview (2), Published (3), Cancelled (4), Released (5), Expired (6) e Migrated (7). `AppearanceRevision.AppearanceRevisionOriginId` substitui `Origin`: Publish (1), Rollback (2) e Migration (3). `ThemeRelease.AppearanceChannelId` substitui `Channel`: store (1) e drop (2). As entidades possuem navegações para as respectivas classificações e FKs com exclusão restrita. As respostas continuam expondo os mesmos códigos textuais em state, origin e channel.

`Appearance.AppearanceTargetTypeId` referencia a entidade estática `AppearanceTargetType`, com navegação `AppearanceTargetType`. `DropEventId` é obrigatório e positivo apenas em `DropSpecific`; nos tipos `Store` e `DropDefault`, deve ser nulo. O Store aceita apenas o tipo 1; o Drop aceita os tipos 2 e 3. Essas regras são protegidas por validação de domínio, validação do módulo e check constraints no banco. Índices únicos parciais garantem uma aparência padrão por empresa/tipo e uma por empresa/tipo/evento, inclusive quando `DropEventId` é nulo. A FK composta de empresa/evento é configurada somente no DbContext do Drop.

`TargetKey` não é mais persistido. Os textos públicos de `target` (`store`, `drop-default`, `drop-event:ID`) e `source` continuam sendo calculados a partir dos códigos da classificação, preservando os contratos HTTP. O catálogo compartilhado define somente as opções; existência, visibilidade, vínculo à empresa, fallback e fixação de revisões dos eventos continuam pertencendo ao Drop.

As migrations `AppearanceTargetTypes` classificam os registros existentes antes de remover a chave textual, preservando IDs, configurações e referências às sessões/revisões. Dados com alvos desconhecidos ou incompatíveis com o módulo interrompem a migration e precisam ser corrigidos antes da aplicação. A reversão recompõe as chaves textuais originais a partir do tipo e do evento.

## Temas da plataforma

A equipe da plataforma define os presets no projeto do frontend e publica seus contratos no backend. O lojista escolhe um tema cadastrado e personaliza seus campos permitidos. O frontend monta as telas; o backend valida e persiste os contratos e as escolhas da loja. Esta entrega não implementa o renderer Vue ou o editor administrativo.

Não existem pacotes JSON de temas embarcados nem importação automática ao iniciar a aplicação, consultar o catálogo ou editar uma aparência. Um banco novo retorna catálogo vazio até receber as definições da plataforma. Registros de temas já persistidos continuam disponíveis para suas aparências e revisões.

Cada módulo recebe o catálogo em `POST /api/internal/appearance-themes`, com um `InputThemeCatalog` contendo a lista `definitions`. Cada `InputThemeDefinition` informa `channel`, `key`, `version`, `name`, `rendererContract`, `schema` e `defaults`; `replacementVersion`, `migrationDeadline` e `migration` são opcionais para depreciação. O Store aceita apenas `channel: "store"`, e o Drop apenas `channel: "drop"`.

`catalogPosition` é a posição comercial estável do modelo, compartilhada por todas as suas
versões no canal. A posição 1 é o padrão e precisa existir no catálogo. Sem posição explícita,
uma chave nova recebe a próxima posição; uma versão nova reutiliza a posição da chave.
Não é permitido mover uma chave existente ou atribuir sua posição a outra chave.
O catálogo pode conter modelos além da capacidade de um plano; isso não os libera para esse plano.

A publicação exige `X-Service-Key`, validado contra `Security:InternalServiceKey`, seguindo a autenticação de serviço do projeto. A chamada deve partir do servidor ou da automação de publicação do frontend da plataforma. Essa chave permanece no ambiente confiável da plataforma e não deve ser enviada ao navegador do lojista. Um token de dono de loja não autoriza cadastrar temas globais.

O endpoint aceita até 50 definições por chamada, com limite total de 1 MiB, e responde 204 quando conclui. `RegisterAppearanceThemesCommandHandler` valida as entradas e registra todas as definições em uma transação. O reenvio do mesmo conteúdo é idempotente; alterar uma combinação channel/key/version já registrada causa `Appearance.PackageChanged`. Nome, schema, defaults e contrato do renderer participam do checksum. Informações de depreciação podem evoluir sem alterar esse conteúdo.

As definições usam JSON Schema 2020-12, com objetos fechados por `additionalProperties: false`. A versão inicial aceita releases estáveis no formato major.minor.patch e rendererContract 1. Os schemas são autocontidos, sem `$ref`. Metadados `x-ui` podem descrever controles do editor; não são componentes ou código executável.

A validação usa JsonSchema.Net 7.3.3, cujo pacote declara licença MIT. A versão foi fixada para não introduzir as condições de manutenção comercial encontradas no pacote binário 9.4.0. Referência do pacote: [NuGet](https://www.nuget.org/packages/JsonSchema.Net/7.3.3).

Exemplos de campos que podem ser definidos pelo frontend:

- Nome público e referência opcional de logo.
- Cores em formato hexadecimal de seis dígitos.
- Fonte de uma lista aprovada, opção booleana e raio numérico limitado.
- IDs de produtos em destaque; no Store, categoria opcional.

Preço, estoque, elegibilidade, datas de campanha e posição na fila continuam pertencendo aos domínios comerciais. Produtos/categorias são referências, sem cópia desses dados no manifesto visual. No Drop, uma configuração de evento aceita apenas produtos ativos da loja que pertençam ao evento. O padrão de drops pode referenciar produtos da própria loja.

## Direitos comerciais de aparência — Parte 4

O servidor consulta os direitos atuais por loja e canal. Store aplica 1/3/10/20 modelos
para Gratuito/Inicial/Pro/DRPCommerce; DRPDrop não inclui Store. No canal DROP, DRPCommerce
e DRPDrop incluem dez modelos. Os demais não incluem aparência DROP. Versões de uma mesma
chave não consomem outro modelo. O catálogo administrativo retorna apenas os modelos
permitidos, com `catalogPosition`; pode listar várias versões da mesma chave.

`GetStatus` inclui `capabilities`: loja, canal, revisão do contrato, disponibilidade do
módulo, limite de modelos e permissões de logo/personalização completa. O contrato é
revalidado ao escolher/iniciar edição, salvar, gerar prévia, publicar, restaurar e gravar
no banco. Cancelar uma sessão ou consultar suas configurações/revisões anteriores continua
possível. Uma sessão aberta antes de um downgrade não preserva o direito de publicar.

Metadados comerciais do JSON Schema, mantidos pela plataforma:

- `x-plan: "business"`: campo comercial necessário, disponível também no Gratuito;
- `x-plan: "logo"`: referência `x-resource: "asset"` de logo, disponível a partir do Inicial;
- `x-plan: "full"` ou ausência de anotação: personalização completa dos planos superiores.

As anotações são validadas no registro e podem ser aplicadas a campos comerciais de objetos.
`logo` exige uma referência de ativo. Referências de ativo não herdam liberação de um objeto
`business`. Por compatibilidade, `brand.name` é comercial e `brand.logoAssetId` é logo mesmo
sem a nova anotação. Gratuito usa o modelo padrão/defaults e preserva os campos comerciais;
Inicial pode escolher seus três modelos e trocar o logo, mantendo os demais campos nos
defaults. Os superiores podem alterar todos os campos declarados pelo schema fechado.

Uma publicação fora dos direitos atuais deixa de ser exposta. A leitura usa a última revisão
permitida daquela aparência, até o número da revisão efetiva, e depois o modelo padrão,
se necessário. A busca não se limita à primeira página do histórico. A configuração pública
também projeta os campos permitidos: cor/fonte/logo antigos não contornam a redução de plano.
Os documentos salvos e os ponteiros de publicação são preservados, sem reescrever o histórico.
Recuperar os direitos pode tornar a publicação preservada elegível novamente; uma publicação
nova feita pelo dono passa a ser a selecionada normalmente.

Uma URL pública por revisão fora do plano retorna 404. Em modelo ainda permitido, a revisão
usa a projeção atual dos campos e um checksum/ETag correspondente. O histórico completo
continua consultável pelo dono, enquanto rollback revalida os direitos da revisão escolhida.

No Store, aparência e mudança de contrato compartilham o lock de Enterprise, antes do lock
visual. A gravação revalida também o dono atual. No DROP, `IStoreAppearanceReader` consulta
o Store por API autenticada de serviço, incluindo na persistência e na leitura pública.
Falha na consulta não libera direitos presumidos. Não há transação distribuída entre os
dois bancos; a leitura pública sempre revalida o contrato atual, inclusive após uma gravação
que tenha concorrido com alteração no Store. Sincronização durável do fluxo DROP continua
pertencendo à Parte 6.

Erros adicionais: `Appearance.ThemeNotAllowed`, `Appearance.CustomizationNotAllowed`,
`Appearance.CatalogPositionConflict`, `Appearance.InvalidCommercialMetadata`,
`Appearance.DefaultThemeRequired` e `Appearance.ContractUnavailable`.

## Autorização e tenant

O dono principal autenticado da loja selecionada pode gerenciar aparência. Uma sessão só pode ser alterada por seu editor; o dono atual também pode liberá-la explicitamente, por exemplo após transferência de propriedade.

As rotas públicas estão sob `/api/storefront/{storeSlug}`, reutilizando a resolução de domínio ativo/verificado. IDs recebidos no body não escolhem um tenant. Um token de outra loja continua incompatível com o domínio acessado.

A resolução de domínio e a validação de sessão do Drop continuam usando a integração de segurança preexistente com Store. A leitura pública de aparência usa o banco do próprio módulo e não busca configurações visuais ou referências de produto no outro serviço.

A validação de referências durante publicação/migração do Drop pode consultar produtos canônicos no Store. Para manutenção sem requisição HTTP, StoreIntegrationScope transporta o tenant interno pelo fluxo assíncrono; ele não substitui o tenant de um usuário autenticado e não recebe valores de cabeçalhos públicos.

## Edição e publicação

1. BeginEdit adquire o bloqueio transacional do tenant e cria uma sessão exclusiva para o alvo.
2. A sessão começa com o documento publicado efetivo, quando é da mesma release solicitada, ou com os defaults. `useDefaults: true` pede os defaults explicitamente.
3. Autosave envia o documento completo. O servidor valida o schema, incrementa a versão, invalida o preview e renova a expiração para 24 horas de inatividade.
4. Preview repete a validação de schema/recursos e devolve um manifesto privado. Preview também incrementa a versão da sessão; usar seu novo ETag nas chamadas seguintes.
5. Publish repete as validações, insere uma revisão imutável, troca a publicação e fecha a sessão em uma única transação.
6. Cancel descarta a edição sem mudar a publicação. Uma sessão expirada não pode mais ser salva/publicada e é fechada ao iniciar a próxima edição.
7. Rollback valida novamente uma publicação anterior e cria outra revisão auditável, sem alterar o registro histórico.

Há advisory locks transacionais PostgreSQL por tenant e índices únicos por alvo/sessão aberta. A concorrência otimista complementa esses bloqueios: cada mutação da sessão exige If-Match forte com a versão esperada. Duas tentativas de BeginEdit não criam duas sessões abertas. Alterações em alvos distintos podem ter sessões independentes; as transações curtas da mesma loja são serializadas.

## Rotas dos serviços

Os endereços abaixo são internos às APIs, seguindo os prefixos existentes do projeto. Com a configuração atual do Gateway, acrescente `/api/store` ou `/api/drop` antes da rota inteira. Por exemplo, o caminho externo do catálogo Store é `/api/store/api/appearance-themes`.

### Administração

| Alvo | Base |
|---|---|
| Vitrine no Store | /api/appearances |
| Padrão de drops no Drop | /api/appearances |
| Evento no Drop | /api/drop-events/{eventId}/appearance |

| Método e sufixo da base | Operação |
|---|---|
| GET sem sufixo | Status, revisão publicada e eventual bloqueio de migração |
| GET edit-session | Sessão aberta; 204 se ausente/expirada |
| POST edit-sessions | Iniciar edição com themeKey, themeVersion e useDefaults |
| PUT edit-sessions/{sessionId}/settings | Salvar `{ "settings": { ... } }` completo; exige If-Match |
| POST edit-sessions/{sessionId}/preview | Preview privado; exige If-Match |
| POST edit-sessions/{sessionId}/publish | Publicar; exige If-Match |
| POST edit-sessions/{sessionId}/cancel | Cancelar pelo editor; exige If-Match |
| POST edit-sessions/{sessionId}/release | Liberar bloqueio pelo dono principal; exige If-Match |
| GET revisions | Até 100 revisões, da mais recente para a mais antiga |
| POST revisions/{revisionId}/rollback | Restaurar com `{ "expectedRevisionId": ID_ATUAL }` |
| GET migration/preflight | Validar a migração prevista sem alterar produção |
| POST migration/retry | Tentar novamente a migração após uma correção |

`GET /api/appearance-themes` lista as releases do módulo, com seus schemas, defaults e estado de depreciação. Para continuar a paginação de revisões, envie `?beforeNumber=NUMERO_DA_ULTIMA_REVISAO_RECEBIDA`. O histórico não é removido quando deixa de aparecer na primeira página.

### Consulta pública

| Módulo | Rota |
|---|---|
| Store | GET /api/storefront/{storeSlug}/appearance |
| Store, revisão histórica | GET /api/storefront/{storeSlug}/appearance/revisions/{revisionId} |
| Drop, padrão da loja | GET /api/storefront/{storeSlug}/drop/appearance |
| Drop, revisão do padrão | GET /api/storefront/{storeSlug}/drop/appearance/revisions/{revisionId} |
| Drop, evento | GET /api/storefront/{storeSlug}/drop/events/{slug}/appearance |
| Drop, revisão histórica do alvo | GET /api/storefront/{storeSlug}/drop/events/{slug}/appearance/revisions/{revisionId} |

O manifesto contém manifestVersion, revisionId, channel, source, identificação da release, settings, assetBindings, resourceBindings, checksum e publishedAt. Ele não contém schema/editor/tenant interno ou cópia de trabalho. Revisões históricas ficam restritas à mesma loja e ao alvo ou seu padrão aplicável.

## Integração do frontend

Exemplo de início de edição no Store:

```http
POST /api/appearances/edit-sessions
Authorization: Bearer ACCESS_TOKEN_ADMINISTRATIVO
Content-Type: application/json

{
  "themeKey": "chave-do-tema-publicado-pela-plataforma",
  "themeVersion": "1.0.0",
  "useDefaults": false
}
```

A resposta fornece sessionId, settings e ETag, por exemplo `"1"`. Ao salvar o documento completo, enviar esse ETag em If-Match. Usar o novo ETag retornado; não continuar gravando a versão anterior. O CORS passa a expor ETag e Retry-After às origens já autorizadas.

O renderer escolhe um componente compilado por `theme.key + theme.version`, aplica os campos conhecidos e consulta os dados comerciais separadamente. O Swagger das APIs publica os endpoints de aparência, os DTOs e o parâmetro If-Match.

## Drop: padrão, override e campanha ativa

Cada empresa tem um padrão Drop. Uma aparência própria de evento substitui o documento inteiro; não existe merge em tempo de leitura.

Ao criar/atualizar um evento para Ativo, Esgotado ou Encerrado a partir de um estado anterior, a gravação captura a revisão do override ou do padrão, na mesma transação do evento. Alterar o padrão posteriormente não muda esse evento. Uma publicação explícita no alvo do evento pode mudar sua revisão efetiva.

A revisão fica em DropEvent.EffectiveAppearanceRevisionId e tem FK composta por tenant. Eventos novos que já entram ativos também são cobertos. Migrações automáticas de temas não trocam a revisão efetiva de campanhas já fixadas; manter renderers antigos enquanto houver referências publicadas/efetivas.

Eventos legados ou ativados sem publicação podem continuar sem revisão fixada. A leitura
pública pode usar os defaults do modelo padrão cadastrado e permitido, sem criar uma revisão
ou fixar um novo ponteiro no evento. Sem modelo padrão disponível, retorna 404. Portanto,
cadastre o contrato do modelo padrão e entregue seu renderer antes de apresentar o evento.

## Depreciação e migração

Um pacote pode declarar replacementVersion, migrationDeadline UTC e migration:

```json
{
  "replacementVersion": "1.1.0",
  "migrationDeadline": "2027-01-01T00:00:00Z",
  "migration": {
    "rename": { "options.oldName": "options.newName" },
    "remove": ["options.obsolete"]
  }
}
```

O substituto precisa ser uma versão posterior do mesmo tema/canal e estar registrado. A migração suporta renomeação de campos em objetos existentes, remoção e aplicação dos defaults novos; não executa scripts. Uma renomeação não sobrescreve silenciosamente um campo já existente.

O worker verifica até 100 alvos por ciclo de um minuto. Ao vencer a janela, ele valida a transformação e as referências, publica uma revisão com origem Migration e fecha uma sessão eventualmente aberta. Falha de validação faz rollback, mantém a publicação anterior e registra MigrationBlocked/MigrationFailureCode para correção. GetStatus expõe essa pendência somente à administração. Depois de corrigir o pacote/recurso, use migration/retry. Falhas de infraestrutura são registradas e tentadas novamente nos ciclos seguintes.

Não remover releases antigas do frontend apenas porque o padrão migrou: campanhas ativas ainda podem usar revisões anteriores.

## Cache e falhas

- Manifestos atuais: ETag e `public, max-age=0, must-revalidate` para visitantes anônimos. If-None-Match compatível retorna 304.
- Rotas por revisão: também `must-revalidate` em Store e DROP. Plano e disponibilidade de recursos podem mudar sem outra revisão visual; não são URLs imutáveis de conteúdo público.
- Administração e requisições autenticadas: no-store.
- Qualquer cache adicional precisa preservar slug da loja, alvo e revisão. O estado de fila, reserva e pedido permanece privado.

Erros relevantes: 400 para configurações/referências inválidas; 401/403 para identidade/permissão; 404 para alvo/revisão indisponível; 409 para bloqueio/estado incompatível; 412 para versão antiga; 428 para ausência de If-Match. Respostas esperadas usam o tratamento de falhas existente das APIs.

## Migrations e validação

Foram adicionadas migrations Appearance independentes em StoreCommerce.Infrastructure.Data e DropCommerce.Infrastructure.Data. Elas criam as tabelas visuais; Drop também recebe a referência efetiva em evento e uma chave composta. Nenhuma migration histórica foi reescrita.

As migrations `AppearanceStaticEntities` criam e preenchem os três catálogos estáticos, convertem os códigos textuais existentes para suas FKs e só então removem as colunas antigas. Um código desconhecido interrompe a operação sem substituir silenciosamente seu valor. A reversão recupera os códigos a partir dos catálogos antes de removê-los. A conversão e a reversão foram validadas com registros de todos os estados, origens e canais em bancos temporários dos dois módulos.

A aplicação não executa Database.Migrate automaticamente. Aplique as migrations no ambiente escolhido antes de usar as APIs de aparência. As migrations foram validadas em bancos temporários; os bancos existentes do desenvolvedor não foram alterados nesta entrega.

Build sem instalar/alterar hooks locais:

```powershell
dotnet build DROP-Ecommerce/DROP-Ecommerce.sln -p:InstallGitHooks=false
```

## Logos e mídia — Parte 4

O Store mantém os logos em PostgreSQL (`ativo_midia`, binário `bytea`), por loja, com ID,
GUID público aleatório, hash e dimensões. Não grava arquivos no repositório e não depende
de bucket externo para o piloto. O DROP usa o leitor remoto do Store. `IMediaAssetReader`
possui adapters ativos nos dois módulos; URLs arbitrárias não substituem uma referência de ativo.

| Método | Rota Store | Acesso / resultado |
| --- | --- | --- |
| POST | /api/media-assets | Dono, multipart/form-data com campo `file`; cria ou reutiliza logo idêntico |
| GET | /api/media-assets | Dono; últimos 100 ativos, sem binários, com URL e indicador de retenção DROP |
| GET | /api/media-assets/{id}/content | Dono da loja; consulta privada do binário |
| DELETE | /api/media-assets/{id} | Dono; remove upload sem referências preservadas |
| GET | /api/storefront/{storeSlug}/media/{publicId} | Slug da loja, GUID público e direito de logo vigente |
| GET | /api/internal/media-assets/{id} | Serviço autenticado da loja; referência do logo |
| POST | /api/internal/media-assets/{id}/retain-for-drop | Serviço autenticado, módulo DROP vigente; retém o ativo e retorna a referência |
| GET | /api/internal/enterprises/{id}/appearance-rights/{channel} | Serviço autenticado da mesma loja; direitos atuais do canal |

Uploads aceitam PNG estático sem entrelaçamento, até 1 MiB, até 2048 pixels por lado e
1.048.576 pixels no total. A validação confere assinatura, chunks/CRC, dimensões, fluxo zlib
e tamanho das linhas, com descompressão limitada; remove metadados opcionais. SVG, animação,
arquivo truncado, payload adicional e PNG corrompido são recusados. A estrutura segue a
[especificação PNG do W3C](https://www.w3.org/TR/png/). A detecção usa os bytes, sem confiar
na extensão ou no Content-Type informado pelo navegador.

O limite técnico atual é 25 MiB de logos ativos por loja. O hash do conteúdo normalizado
evita duplicar upload idêntico dentro da loja. Não é um adicional comercial de armazenamento.
A resposta informa `id`, `publicId`, dimensões, bytes, data, `url` e `isRetainedByDrop`.
Enviar esse `id` em `brand.logoAssetId`; os manifests/previews retornam também a URL em
`assetBindings.primaryLogo`. URLs são relativas à API Store: o frontend que usa o Gateway
precisa compor o prefixo `/api/store`, conforme a configuração existente.

O endereço público por GUID destina-se à exibição do logo, inclusive antes da publicação
para prévia, por quem conhece essa URL. Não é armazenamento privado de documentos.
Seu acesso revalida a loja resolvida pelo slug e o direito de logo, serve somente `image/png`, com `nosniff`
e cache que exige revalidação. Ao perder o direito, a aparência pública remove o logo e a
URL deixa de ser servida; o dono mantém consulta privada ao ativo preservado.

Não é possível excluir logo usado por uma edição aberta ou qualquer revisão Store
preservada. Upload sem uso é excluído logicamente e seu binário é limpo, liberando a cota.
No DROP, a validação/prévia/publicação retém o ativo no Store antes do uso, de forma
idempotente. Essa retenção impede que o Store apague um logo referenciado apenas no banco
DROP. É conservadora: uma tentativa de prévia/publicação que falhe depois da retenção
também mantém o ativo retido. Não há liberação automática de retenção nesta etapa.

As escritas exigem escopo próprio de mídia, mesmo em operações de identidade. Conteúdo
publicado não é substituído no mesmo ID. Eventos de upload, exclusão e retenção registram
atividade sem copiar o binário. Erros principais: `Media.PlanRequired`, `Media.InvalidPng`,
`Media.UnsupportedPng`, `Media.TooLarge`, `Media.DimensionsExceeded`, `Media.StorageLimit`
e `Media.InUse`.

Migrations da Parte 4: Store `20261120025127_AddAppearanceEntitlementsAndMedia` e
`20261120030208_RetainLogosReferencedByDrop`; DROP `20261120025225_AddAppearanceThemeEntitlements`.
Releases legadas recebem uma posição por chave/canal, na ordem da primeira versão cadastrada,
sem alterar versões, schemas, defaults ou checksums. Atualize Store/migrations e suas rotas
internas antes do DROP que passa a consultá-las. Foram exercitadas somente em bancos
descartáveis de teste, sem migração dos bancos existentes.

## Fora desta entrega

Os limites operam sobre os modelos realmente cadastrados pela plataforma. Os testes usam
contratos de exemplo; não instalam vinte renderers visuais no frontend nem publicam temas
automaticamente em bancos existentes. Renderer, editor e catálogo visual reais precisam
ser entregues pelo frontend usando os contratos versionados.

O módulo de aparência não implementa frontend, editor Vue, SSR ou operação comercial. O Store já possui [estoque por SKU](../Store/Application/ProductsAndInventory.md) e [checkout/pedidos](../Store/Application/OrdersAndCheckout.md); o ciclo completo de fila/venda do Drop e os provedores externos continuam pendentes. A aparência apenas apresenta esses dados e estados.
