# Shared — estrutura comum de Store e Drop

A solution agrupa as bibliotecas comuns em `4. Shared`. Os fontes ficam em `DROP-Ecommerce/Shared`, organizados nas mesmas camadas dos módulos:

| Projeto | Responsabilidade e pastas |
|---|---|
| `SharedCommerce.Domain` | `Entity/<Entidade>`, `Entity/Base`, `StaticEntity/<Classificação>` e `Exceptions`. Contém entidades, classificações fixas e falhas, sem depender de contratos HTTP, Application, EF ou ASP.NET. |
| `SharedCommerce.Application` | `Features/Commands/<Entidade>/<Operação>`, consultas em `Queries` dentro da entidade, notificações em `Features/Notifications`, `Contracts/Appearance`, `Interfaces`, `Mappings`, `Validation`, `Security` e `Options`. |
| `SharedCommerce.Infrastructure.Data` | `Configurations/<Entidade>`, `Repositories`, `Context`, `Providers`, `Workers` e `Exceptions`. Implementa a persistência e a classificação de falhas de banco. |
| `SharedCommerce.Api` | `Controllers` e `Configuration`. Fornece os controllers-base e a configuração HTTP comum de JWT, autorização, CSRF, CORS, proxies e limites de requisições. |

Appearance e Security compartilham essas camadas. Não há projetos separados de Contracts ou Security.Shared, pasta Models ou arquivos compilados por links entre projetos. Cada tipo público possui seu próprio arquivo. Os contratos de entrada e saída pertencem a Application; o Domain recebe apenas valores de domínio.

O sentido das dependências é `Api → Application → Domain` e `Infrastructure.Data → Application → Domain`. O Shared não referencia projetos de Store ou Drop. `SharedCommerce.Api` é uma biblioteca usada pelas APIs existentes; não cria outro serviço executável.

## Composição dos módulos

`AddSharedApplication` registra os handlers comuns junto aos assemblies informados pelos módulos e os validadores de aparência. O Drop mantém a substituição explícita de `ResolvePublishedAppearanceQuery` pelo seu handler de eventos após esse registro.

`AddSharedInfrastructure` registra o escopo de gravação, o leitor de mídia desabilitado, o relógio e a manutenção de aparência. Cada módulo continua registrando seu próprio DbContext, tenant, contexto de autorização, recursos e repositório de aparência com o canal correspondente.

`AddCommerceSecurity` e `UseCommerceSecurity` pertencem a `SharedCommerce.Api/Configuration`. `StoreCommerce.Security` e `DropCommerce.Security` fornecem as implementações próprias de identidade, sessão, domínio e comunicação interna. Não há referência de um módulo à implementação do outro.

As entidades e classificações de aparência mantêm IDs, códigos, tabelas, relacionamentos, filtros e regras de gravação. Os snapshots atuais do EF acompanham os namespaces novos; as migrations históricas são preservadas. Esta reorganização não exige migration nem alteração nos bancos existentes.

Detalhes funcionais: [Appearance](Appearance.md) e [Security](Security.md).
