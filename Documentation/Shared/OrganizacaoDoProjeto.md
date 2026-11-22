# Organização do projeto

As pastas de Store, DROP e Shared são agrupadas por contexto em inglês. A organização
física não altera rotas, JSON, entidades persistidas, regras, transações ou ordem de
execução. A revisão dos Services e das responsabilidades de negócio é uma tarefa
separada desta organização.

## Application

Cada feature pertence a `Features/Commands/<Context>/<Feature>`. Quando o próprio
contexto já identifica a feature, a segunda pasta é dispensada. Exemplos:

```text
StoreCommerce.Application/
  Features/Commands/
    Catalog/Product/
      Create/
        CreateProductCommand.cs
        CreateProductCommandHandler.cs
        CreateProductCommandValidator.cs
      Queries/SearchProducts/
        SearchProductsQuery.cs
        SearchProductsQueryHandler.cs
        SearchProductsQueryValidator.cs
      Contracts/Requests/
      Contracts/Responses/
      Support/
    Orders/Checkout/PlaceOrder/
    Orders/Order/Services/
    Identity/Authentication/
    StoreManagement/Enterprise/
    Commercial/EnterpriseContract/
    Billing/PlatformBilling/
    Inventory/
  Interfaces/<Context>/
  Enums/<Context>/
  Integrations/Providers/
  Behaviours/
```

Cada Command, Query, Handler, Validator, DTO, interface e enum tem seu próprio
arquivo. Partes de um mesmo tipo `partial` podem continuar em arquivos próprios,
como `UserResponse.cs` e `UserResponse.Mapping.cs`.

Contratos de entrada e saída, Services existentes, policies, mappers, validadores
auxiliares, opções e escopos ficam junto de sua feature em subpastas específicas.
Interfaces ficam em `Interfaces/<Context>`; enums ficam em `Enums/<Context>` da
Application ou do Domain, conforme a camada em que já eram definidos.
`Behaviours` contém somente o pipeline transversal.

Integrações técnicas são a exceção à localização das features. Elas ficam em
`Integrations/<Provider>`, com pastas como `Interfaces`, `Contracts`, `Clients`,
`Authorization`, `Notifications`, `Payments`, `Security` e `Options`, conforme a
necessidade. Commands e Queries de gestão da integração continuam nas features.

## Contextos

| Contexto | Responsabilidades agrupadas |
| --- | --- |
| `Catalog` | Produtos, variações, categorias, fornecedores e serviços |
| `Orders` | Carrinho, checkout, pedidos, itens, cupons e envio |
| `Identity` | Contas, autenticação, clientes e exclusão de conta |
| `AccessControl` | Perfis de acesso, permissões e associações |
| `StoreManagement` | Loja, equipe, domínios, convites, propriedade e configurações |
| `Commercial` | Planos, contratos, direitos e reativação |
| `Billing` | Assinatura da plataforma, cobrança e faturas |
| `Payments` | Pagamentos, transações, tentativas e operações financeiras |
| `Inventory` | Estoque, reservas e transferências no DROP |
| `DropIntegration` | Casos de uso de sincronização e transferência no Store |
| `Events` | Eventos e inscrições do DROP |
| `Queue` | Entrada, sessão de compra e lista de espera do DROP |
| `Monitoring` | Auditoria e sinais de fraude |
| `Notifications` | Notificações do DROP |
| `Appearance` | Aparência, temas e publicação |
| `Media` | Arquivos, logos e retenção de mídia |
| `Documents` | Documentos comerciais |
| `Common` | Bases e suporte transversal sem contexto de negócio próprio |

## Shared e demais camadas

Tipos usados pelos dois módulos pertencem a Shared na camada correspondente.
O resultado das operações (`Result`, `Error`, `IResult`, `ErrorType` e extensões
do mediator) é único em `SharedCommerce.Application`. Os contratos de tenant e
exclusão lógica (`ITenantProvider` e `ISoftDeletable`) são únicos em
`SharedCommerce.Domain`.

As features compartilhadas de aparência ficam em
`SharedCommerce.Application/Features/Commands/Appearance`. Os contratos HTTP
entre módulos ficam em `SharedCommerce.Application/Integrations/Drop/Contracts`,
separados por contexto. Componentes que dependem das entidades ou políticas
específicas de um módulo continuam nesse módulo.

As pastas `Entity`, `StaticEntity`, `Configurations`, `Repositories`, `Providers`,
`Workers` e `Controllers` seguem os contextos correspondentes. Arquivos parciais
dos DbContexts são agrupados por área dentro de `Context`. Migrations geradas
permanecem nos caminhos originais.

Os namespaces públicos existentes das features e entidades foram preservados.
Interfaces e enums usam namespaces específicos; os consumidores e os registros
de injeção de dependência acompanham as novas referências.
