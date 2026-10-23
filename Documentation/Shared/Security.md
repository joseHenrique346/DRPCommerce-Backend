# Organização dos projetos de segurança

Os casos de uso de identidade seguem o fluxo `Controller → MediatR → Handler → Repository/UnitOfWork`. As bibliotecas Security fornecem as capacidades de segurança usadas pelos handlers.

## Responsabilidades

| Camada/projeto | Responsabilidade |
|---|---|
| StoreCommerce.Api | Rotas, cookies, CSRF, políticas HTTP e conversão do Result para o contrato público |
| StoreCommerce.Application | Commands/Queries, handlers, validators, DTOs e contratos de serviços/persistência |
| StoreCommerce.Domain | Entidades, relacionamentos e contratos dos repositórios |
| StoreCommerce.Infrastructure.Data | Consultas EF, repositórios, transações e gravação no banco |
| StoreCommerce.Security | Hash de senha, JWT, contexto autenticado, resolução de tenant, verificação DNS e entrega de ações |
| DropCommerce.Security | Validação da sessão pelo Store, resolução de tenant e autenticação dos clients internos |
| SharedCommerce.Domain | Falhas comuns de domínio e segurança |
| SharedCommerce.Application | Interfaces, opções e primitivas comuns de segurança |
| SharedCommerce.Infrastructure.Data | Classificação comum de falhas de persistência |
| SharedCommerce.Api | Registro e pipeline HTTP comum de segurança |

Essas quatro bibliotecas ficam no grupo `4. Shared`, junto da aparência, conforme a [estrutura do Shared](README.md). Os arquivos comuns são compilados apenas no projeto responsável, sem `Compile Include` entre módulos.

## Pastas do StoreCommerce.Security

- `Services/Sessions`: emissão e validação JWT, consultando contratos de repositório.
- `Hashers`: cálculo e verificação de hashes de senha.
- `Context`: implementação de `ICurrentIdentity` a partir da requisição HTTP.
- `Scopes`: escopo de operações confiáveis de identidade.
- `Resolvers`: resolução da loja pelo domínio ou pelo contexto interno autenticado.
- `Verifiers`: verificação de propriedade de domínio por DNS.
- `Delivery`: entrega de ações de conta; permanece desabilitada nesta etapa.
- `Workers`: agendamento da limpeza; as consultas de remoção pertencem à infraestrutura.

As antigas classes IdentityService e AccessManagementService foram substituídas por handlers específicos em Application, não apenas movidas de pasta.

## Dependências

StoreCommerce.Security referencia Application, Domain e a biblioteca HTTP SharedCommerce.Api, sem referenciar Infrastructure.Data nem usar DbContext. Application depende de contratos, não de Entity Framework.

O Drop mantém sua referência à infraestrutura para os clients da integração existente. A comunicação Store/Drop continua por HTTP; uma API não referencia a infraestrutura do outro módulo.

As APIs registram segurança com AddStoreSecurity/AddDropSecurity. Repositórios e transações são registrados em AddStoreInfrastructure.

## Repositórios e transações

Repository<T> continua sendo a base dos CRUDs. Seus métodos são virtuais, permitindo overrides. A base de consultas EntityRepository<T> implementa consultas por expressão sem expor DbContext ou IQueryable aos handlers.

Os repositórios específicos acrescentam bloqueios de registros e consultas de vínculos/permissões. IIdentityRepositories reúne contratos; sua implementação não contém regras de negócio. AuthenticationSessionRepository concentra agrupamento, revogação e limpeza de sessões.

IIdentityUnitOfWork amplia o contrato de gravação com transações. IUnitOfWork e IIdentityUnitOfWork resolvem a mesma instância scoped e o mesmo DbContext. CommitAsync grava alterações; o commit da transação é explícito e separado. Bloqueios FOR UPDATE e a ordem dos commits dos fluxos sensíveis foram preservados.

## Shared

`SharedCommerce.Application/Options` contém `SecurityOptions`; `Security` contém `SecurityNames`, `OpaqueToken` e `HostName`; e `Interfaces` reúne `ISessionValidator` e `IRequestTenantResolver`. Cada tipo fica em seu próprio arquivo.

`SecurityFailure` fica em `SharedCommerce.Domain/Exceptions`, junto das falhas de aparência. O Domain não depende de ASP.NET, EF ou contratos de entrada/saída.

A configuração ASP.NET de autenticação, autorização, cookies e rate limiting é compilada uma vez em `SharedCommerce.Api/Configuration`. `SecurityErrors`, dependente de EF/PostgreSQL, fica em `SharedCommerce.Infrastructure.Data/Exceptions`. As duas APIs consomem essa implementação comum. Application e StoreCommerce.Security não precisam do EF para consumir as primitivas.

## Compatibilidade e escopo

Controllers continuam específicas por entidade/função, com construtor primário e herança na mesma linha. Os handlers retornam Result<T> internamente; a API mantém o DTO direto nos sucessos de identidade, sem acrescentar envelope ao login.

Cookies, CSRF e respostas 204/202 continuam na API e só são produzidos após sucesso. Falhas esperadas viram Result preservando código/status; falhas de infraestrutura continuam no exception handler global.

Filtros de tenant, guards de gravação e matriz de permissões não foram removidos. Esta etapa reorganiza identidade/acesso; não reescreve os CRUDs comerciais do Drop nem as consultas internas/storefront que não pertencem a esses fluxos.

Não há novas migrations, bancos permanentes, serviços de rede ou portas adicionais. Veja [Casos de uso de identidade](../Store/Application/IdentityFlows.md).
