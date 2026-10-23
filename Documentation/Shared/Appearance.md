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

## Autorização e tenant

O dono principal autenticado da loja selecionada pode gerenciar aparência. Uma sessão só pode ser alterada por seu editor; o dono atual também pode liberá-la explicitamente, por exemplo após transferência de propriedade.

As rotas públicas estão sob `/api/storefront`, reutilizando a resolução de domínio ativo/verificado. IDs recebidos no body não escolhem um tenant. Um token de outra loja continua incompatível com o domínio acessado.

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
| Store | GET /api/storefront/appearance |
| Store, revisão histórica | GET /api/storefront/appearance/revisions/{revisionId} |
| Drop, padrão da loja | GET /api/storefront/drop/appearance |
| Drop, revisão do padrão | GET /api/storefront/drop/appearance/revisions/{revisionId} |
| Drop, evento | GET /api/storefront/drop/events/{slug}/appearance |
| Drop, revisão histórica do alvo | GET /api/storefront/drop/events/{slug}/appearance/revisions/{revisionId} |

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

Eventos que já estavam ativos antes desta implantação não possuem revisão fixada. Da mesma forma, ativar um evento sem aparência publicada não inventa um visual. Sua consulta de aparência retorna 404 até uma publicação explícita nesse evento. Portanto, configure/publice o padrão antes de ativar novas campanhas.

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
- Rotas por revisão: cache imutável de um dia. A resolução de domínio/alvo continua sendo exigida em novas requisições.
- Administração e requisições autenticadas: no-store.
- Qualquer cache adicional precisa preservar Host/tenant, alvo e revisão. Não compartilhar estado privado de fila/reserva/pedido.

Erros relevantes: 400 para configurações/referências inválidas; 401/403 para identidade/permissão; 404 para alvo/revisão indisponível; 409 para bloqueio/estado incompatível; 412 para versão antiga; 428 para ausência de If-Match. Respostas esperadas usam o tratamento de falhas existente das APIs.

## Migrations e validação

Foram adicionadas migrations Appearance independentes em StoreCommerce.Infrastructure.Data e DropCommerce.Infrastructure.Data. Elas criam as tabelas visuais; Drop também recebe a referência efetiva em evento e uma chave composta. Nenhuma migration histórica foi reescrita.

As migrations `AppearanceStaticEntities` criam e preenchem os três catálogos estáticos, convertem os códigos textuais existentes para suas FKs e só então removem as colunas antigas. Um código desconhecido interrompe a operação sem substituir silenciosamente seu valor. A reversão recupera os códigos a partir dos catálogos antes de removê-los. A conversão e a reversão foram validadas com registros de todos os estados, origens e canais em bancos temporários dos dois módulos.

A aplicação não executa Database.Migrate automaticamente. Aplique as migrations no ambiente escolhido antes de usar as APIs de aparência. As migrations foram validadas em bancos temporários; os bancos existentes do desenvolvedor não foram alterados nesta entrega.

Build sem instalar/alterar hooks locais:

```powershell
dotnet build DROP-Ecommerce/DROP-Ecommerce.sln -p:InstallGitHooks=false
```

## Fora desta entrega

Upload, processamento e escolha de armazenamento de mídia continuam em avaliação. IMediaAssetReader existe como contrato e sua implementação atual é desabilitada: logoAssetId pode ser null; tentar publicar um ativo sem integração válida retorna Appearance.ResourceUnavailable. Não existe endpoint de upload nem fallback para uma URL arbitrária.

Esta entrega também não implementa frontend, editor Vue, SSR, pagamentos ou o mecanismo transacional de estoque/fila. A aparência apenas apresenta os dados e estados comerciais.
