# Persistência e infraestrutura

Revisado em 03/11/2026 conforme o código integrado do Store. Store e Drop usam **PostgreSQL**, EF Core 9 e `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.0. Não há provider MySQL/Pomelo na configuração atual.

Cada módulo tem banco, DbContext, migrations e repositórios próprios. Shared fornece componentes usados nos dois bancos; não cria um terceiro banco. A comunicação Store/Drop é HTTP autenticada, sem chave estrangeira entre bancos.

## Composição

```text
StoreCommerce.Api / DropCommerce.Api
  → AddApiConfiguration
    → AddStoreInfrastructure / AddDropInfrastructure
      → DbContext com UseNpgsql
      → TenantProvider, repositórios e UnitOfWork
      → AddSharedInfrastructure (aparência e serviços comuns)
    → AddStoreApplication / AddDropApplication
    → AddStoreSecurity / AddDropSecurity
```

Os projetos `StoreCommerce.Security` e `DropCommerce.Security` concentram os adaptadores de identidade. As quatro bibliotecas `SharedCommerce.*` compartilham primitivas e configuração sem referenciar os módulos. Veja [Security](Security.md) e [estrutura do Shared](README.md).

## Configuração dos bancos

As APIs procuram primeiro a variável específica e depois `ConnectionStrings:DefaultConnection` da configuração ASP.NET:

| Serviço | Variável prioritária | Banco sugerido no template |
| --- | --- | --- |
| Store | `STORECOMMERCE_CONNECTION_STRING` | `drpcommerce_store` |
| Drop | `DROPCOMMERCE_CONNECTION_STRING` | `drpcommerce_drop` |

Exemplo de formato, com valores fictícios:

```text
Host=localhost;Port=5432;Database=drpcommerce_store;Username=<USER>;Password=<PASSWORD>;
```

Sem configuração de banco, a inicialização falha explicitamente. Os templates versionados ficam junto das APIs; os arquivos locais com credenciais são ignorados pelo Git. Para comandos de execução e configuração de JWT/chave interna, veja [inicialização](StartupProject.md).

## Modelo atual

Os DbContexts e `Configurations` são a referência completa do schema. Os grupos abaixo substituem o inventário antigo, que não incluía identidade, aparência e o ciclo comercial:

| Módulo / grupo | Entidades principais |
| --- | --- |
| Store — identidade | User, Customer, Enterprise, EnterpriseMembership, EnterpriseDomain, Employee, EmployeePosition, Department, AccessRole, Permission e relações; sessões, tokens, convites e transferência de propriedade. |
| Store — catálogo | Category, Product, ProductVariant, Service e Supplier. |
| Store — operação | StoreSettings, Cart, CartItem, CustomerAddress, Order, OrderItem, OrderStatusHistory, Transaction, Shipment, Invoice e Document. |
| Store — estoque e desconto | StockReservation, StockReservationLine, StockMovement, Coupon, condições de cupom e CouponRedemption. |
| Drop — evento | DropEvent, DropProduct, DropCoupon e DropRegistration. |
| Drop — espera e compra | QueueEntry, QueueSession, WaitlistEntry, DropReservation, DropOrder, DropOrderItem e DropTransaction. |
| Drop — acompanhamento | DropNotification, DropAuditLog e FraudSignal. |
| Compartilhado, nos dois bancos | Appearance, AppearanceRevision, AppearanceEditSession, ThemeRelease e classificações estáticas. |

`EmployeePosition` substitui a nomenclatura antiga de cargo `Role`. `AccessRole` representa autorização e não é o cargo funcional. As entidades estáticas de status, tipos, métodos e operadores têm IDs fixos e seed por migrations; não são estados livres enviados pelo cliente.

Tabelas usam nomes definidos nos mapeamentos e colunas/chaves/índices recebem convenção snake_case. Aparência usa `jsonb` nos documentos de configuração. Preços e dimensões do catálogo usam a precisão definida nos mapeamentos, atualmente `numeric(18,2)`.

Fontes: [StoreCommerceDbContext](../../DROP-Ecommerce/StoreCommerce.Infrastructure.Data/Context/StoreCommerceDbContext.cs), [DropCommerceDbContext](../../DROP-Ecommerce/DropCommerce.Infrastructure.Data/Context/DropCommerceDbContext.cs), [mapeamentos Store](../../DROP-Ecommerce/StoreCommerce.Infrastructure.Data/Configurations), [mapeamentos Drop](../../DROP-Ecommerce/DropCommerce.Infrastructure.Data/Configurations).

## Tenant e filtros de leitura

`TenantProvider` não retorna zero para liberar uma consulta sem contexto. Ele exige loja resolvida/confiável ou token autenticado válido; ausência de contexto produz falha de segurança. No storefront, o host identifica a loja e precisa ser compatível com o token. No backoffice, a membership selecionada determina o tenant. No Store, chamadas internas autenticadas podem usar o tenant confiável de serviço.

No Store, entidades de tenant recebem filtro direto por EnterpriseId; entidades com exclusão lógica acrescentam a visibilidade correspondente. Dependentes como OrderItem, Shipment e Transaction também são filtrados pelos agregados da loja. ProductVariant considera a exclusão do SKU e do produto pai.

No Drop, o isolamento não se limita a DropEvent: os filtros de segurança alcançam produtos, cupons, inscrições, fila, sessões, reservas, pedidos, itens, transações, notificações e auditoria pelas relações apropriadas. Recursos privados também consideram o CustomerId quando o ator é cliente. Os filtros estão no arquivo parcial `DropCommerceDbContext.Security.cs`.

`IgnoreQueryFilters` só aparece em operações explícitas, como autenticação, manutenção e processamento com tenant informado. Não torna uma consulta pública autorizada a escolher outra loja.

## Proteção de gravação e exclusão

Os DbContexts validam tenant, relações e escopos confiáveis antes de salvar. Não basta ter um ID ou ser dono para alterar livremente saldos, permissões, pagamento e histórico pelo CRUD.

- Identidade exige o fluxo próprio de conta, sessão ou gestão de acessos.
- Estoque exige `InventoryWriteScope`; linhas de reserva e movimentações são protegidas contra edição comum.
- Carrinho, endereços e workflow de pedido possuem escopos próprios.
- Aparência exige `AppearanceWriteScope` e as regras de alvo/revisão do módulo.
- Audit logs do Drop não aceitam mutação manual pública.

Os repositórios aplicam `SoftDelete` às entidades que implementam `ISoftDeletable`; outras exclusões seguem a implementação e as relações. Produto, variante e categoria possuem regras adicionais de saldo/reserva/vínculos. Não generalizar remoção física para todo CRUD nem exclusão lógica para toda tabela.

## Repositórios e transações

`Repository<T>` atende CRUD e pode ser especializado. No Store, `IRepository<T>` também amplia `IEntityRepository<T>`, usado em consultas por predicados/projeções; EF e SQL ficam na infraestrutura. Os fluxos comerciais usam repositórios específicos de produtos, inventário, pedidos, carrinho, clientes, endereços, categorias e cupons.

`CommitAsync` da UnitOfWork chama `SaveChangesAsync`. **Salvar alterações e confirmar uma transação explícita são operações distintas.** No Store, `IUnitOfWork` e `IIdentityUnitOfWork` resolvem a mesma instância scoped; `BeginTransactionAsync` e o commit da transação são explícitos. A UnitOfWork do Drop possui seu contrato próprio; alguns fluxos existentes do Drop coordenam a transação no código de persistência/API.

Catálogo, categorias, estoque e ciclo do pedido usam bloqueio transacional da linha da empresa com `FOR UPDATE`. Isso serializa as operações participantes da mesma loja; não é uma garantia de capacidade ilimitada. Chaves únicas, versões e FKs compostas complementam a proteção contra repetição e cruzamento de lojas.

Não há transação distribuída pronta entre Store e Drop. A integração da venda do Drop com a reserva/pedido do Store continua pendente.

## Workers e adaptadores registrados

| Componente | Comportamento atual |
| --- | --- |
| PendingOrderExpirationWorker | A cada minuto identifica pendentes vencidos; o workflow relê pagamento, estado e prazo após o bloqueio antes de cancelar e liberar estoque/cupom. |
| StockReservationExpirationWorker | A cada minuto expira reservas avulsas; reservas de pedidos seguem o workflow do pedido. |
| Limpeza de identidade | Remove dados de sessão/token conforme retenção, preservando detecção de reutilização durante a vida da família. |
| Manutenção de aparência | Executa migrações/manutenção de temas nos fluxos compartilhados. |
| ManualPaymentGateway | Não chama Mercado Pago ou outra instituição; confirmação é manual e o estorno registrado precisa ser executado fora do sistema. |
| Clients Store/Drop | Refit para recursos; validação de sessão e tenant pelo Store; falha de dependência não concede acesso. |

Detalhes: [estoque](../Store/Application/ProductsAndInventory.md), [checkout e pedidos](../Store/Application/OrdersAndCheckout.md), [integração Store/Drop](../Drop/Infrastructure/StoreIntegration.md), [aparência](Appearance.md).

## Migrations

As APIs não aplicam `Database.Migrate` automaticamente. Gerar arquivos, revisar SQL e aplicar ao banco escolhido são passos separados. Esta revisão documental não aplica migrations nem comprova o estado de uma instalação; conferir o histórico de migrations de cada ambiente.

As evoluções recentes do Store incluem:

- `AlignStoreCommerceFieldLimits`, `AddStoreSettings`, `AddStoreEmployeePermissions` e `EnforceUniqueCategorySlug`;
- `AddProductVariantsAndInventory`, `AddCouponConditionsAndRedemptions` e `AddStorefrontCart`;
- `BackfillCreatedAt`, `AddCheckoutAndOrderLifecycle`, `AddOrderNumber` e `AddCustomerAddresses`;
- `ReserveCouponUsesAtCheckout`, ajuste de dados para comprometer usos pendentes de cupom sem mudar o schema.

Ambos os módulos também possuem migrations próprias de aparência. `IdentityAndSessions` protege a importação de dados legados; não inventa o proprietário de uma loja antiga. Duplicidades de slug/SKU e dados incompatíveis podem bloquear migrations e precisam ser corrigidos conforme a mensagem. Não apagar histórico para contornar essas verificações.

Os comandos completos para listar e aplicar migrations, a partir da raiz do repositório, estão em [StartupProject](StartupProject.md). Regras de dados: [identidade](../Store/Domain/Identity.md), [categorias](../Store/Application/Categories.md), [estoque](../Store/Application/ProductsAndInventory.md) e [cupons](../Store/Application/Coupons.md).
