# Infrastructure.Data — Documentação

## Visão Geral

A camada de infraestrutura de dados implementa o acesso ao banco MySQL HeatWave para os dois bounded contexts da aplicação: **DropCommerce** e **StoreCommerce**. Cada módulo possui seu próprio projeto `Infrastructure.Data` com DbContext, repositórios, configurações e extensões de DI independentes.

A integração HTTP do Drop com o Store está documentada em [Drop/Infrastructure/StoreIntegration.md](../Drop/Infrastructure/StoreIntegration.md).

---

## Arquitetura

```
DropCommerce.Api
  └── Configuration/DependencyInjectionConfig.cs
        ├── AddDropInfrastructure(connectionString)
        │     ├── DbContext (MySQL via Pomelo)
        │     ├── TenantProvider (JWT claim)
        │     ├── UnitOfWork
        │     ├── Repository<T> (genérico)
        │     └── Repositórios específicos
        └── AddDropApplication()
              ├── MediatR
              ├── FluentValidation
              └── ValidationBehaviour pipeline
```

Mesma estrutura para StoreCommerce.

---

## Banco de Dados

- **Provider:** MySQL HeatWave via `Pomelo.EntityFrameworkCore.MySql 9.0.0`
- **Bancos separados:** `DropCommerceDb` e `StoreCommerceDb`
- **Convenção de nomes:**
  - Tabelas: português snake_case (ex: `evento`, `item_pedido`, `status_reserva`)
  - Colunas: snake_case automático via convenção global (ex: `EnterpriseId` → `enterprise_id`)
  - Chaves, FKs e índices: também em snake_case

### Tabelas — Drop (31)

| Entidade | Tabela |
|----------|--------|
| DropEvent | `evento` |
| DropProduct | `produto` |
| DropCoupon | `cupom` |
| DropOrder | `pedido` |
| DropOrderItem | `item_pedido` |
| DropReservation | `reserva` |
| DropRegistration | `inscricao` |
| DropNotification | `notificacao` |
| DropTransaction | `transacao` |
| DropAuditLog | `log_auditoria` |
| FraudSignal | `sinal_fraude` |
| QueueEntry | `entrada_fila` |
| QueueSession | `sessao_fila` |
| WaitlistEntry | `entrada_lista_espera` |
| DropEventStatus | `status_evento` |
| DropCouponType | `tipo_cupom` |
| DropOrderStatus | `status_pedido` |
| DropOrderPaymentStatus | `status_pagamento_pedido` |
| DropNotificationType | `tipo_notificacao` |
| DropNotificationStatus | `status_notificacao` |
| DropNotificationChannel | `canal_notificacao` |
| DropReservationStatus | `status_reserva` |
| DropRegistrationStatus | `status_inscricao` |
| DropTransactionType | `tipo_transacao` |
| DropTransactionStatus | `status_transacao` |
| DropTransactionMethod | `metodo_transacao` |
| FraudSignalType | `tipo_sinal_fraude` |
| FraudSeverity | `severidade_fraude` |
| QueueEntryStatus | `status_entrada_fila` |
| QueueSessionStatus | `status_sessao_fila` |
| WaitlistEntryStatus | `status_lista_espera` |

### Tabelas — Store (16)

| Entidade | Tabela |
|----------|--------|
| Enterprise | `empresa` |
| Product | `produto` |
| Category | `categoria` |
| Order | `pedido` |
| OrderItem | `item_pedido` |
| Customer | `cliente` |
| Employee | `funcionario` |
| Role | `cargo` |
| Department | `departamento` |
| Invoice | `nota_fiscal` |
| Shipment | `envio` |
| Transaction | `transacao` |
| Coupon | `cupom` |
| Service | `servico` |
| Document | `documento` |
| Supplier | `fornecedor` |

---

## Connection String

### Prioridade de resolução

1. **Variável de ambiente** — `DROPCOMMERCE_CONNECTION_STRING` / `STORECOMMERCE_CONNECTION_STRING`
2. **appsettings.Development.json** — usado em ambiente local (está no `.gitignore`)
3. **appsettings.json** — fallback com string vazia (falha explícita se nenhuma anterior estiver configurada)

### Configuração local

Copie o template e preencha suas credenciais:

```bash
cp appsettings.Development.Template.json appsettings.Development.json
```

### Configuração de produção/homologação

Defina a variável de ambiente no host/container:

```bash
export DROPCOMMERCE_CONNECTION_STRING="Server=prod-host;Port=3306;Database=DropCommerceDb;User=app;Password=secret;"
export STORECOMMERCE_CONNECTION_STRING="Server=prod-host;Port=3306;Database=StoreCommerceDb;User=app;Password=secret;"
```

---

## Multi-Tenancy

Implementado via **Global Query Filters** do EF Core + claim JWT `EnterpriseId`.

### TenantProvider

Lê a claim `EnterpriseId` do `HttpContext.User` (JWT). Retorna 0 se não encontrar.

### Filtros por módulo

**Drop:**
- `DropEvent`: filtro `enterprise_id == tenant AND is_deleted == false` (única entidade com EnterpriseId)
- Demais ISoftDeletable: filtro `is_deleted == false`

**Store:**
- Entidades com ITenantEntity + ISoftDeletable (Product, Order, Customer, Coupon, Service, Category): filtro `enterprise_id == tenant AND is_deleted == false`
- Entidades com ITenantEntity only (Employee, Invoice, Document, Supplier): filtro `enterprise_id == tenant`

---

## Soft Delete

### Interface `ISoftDeletable`

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    void SoftDelete();
}
```

### Entidades com soft delete

**Drop:** DropEvent, DropProduct, DropCoupon, DropOrder, DropReservation, DropRegistration, DropNotification

**Store:** Product, Order, Customer, Coupon, Service, Category

### Comportamento no Repository

O `DeleteAsync(id)` e `DeleteRangeAsync(ids)` verificam automaticamente:
- Se a entidade implementa `ISoftDeletable` → chama `SoftDelete()` (marca como deletado)
- Senão → faz `Remove()` físico

---

## Repository Pattern

### Interface genérica `IRepository<T>`

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<T>> GetListByListIdAsync(List<long> ids, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    Task DeleteAsync(long id, CancellationToken ct);
    Task DeleteRangeAsync(IEnumerable<long> ids, CancellationToken ct);
}
```

### Repositórios específicos

**Drop:**
- `IDropEventRepository` — GetBySlugAsync, GetActiveEventsAsync
- `IDropOrderRepository` — GetByCustomerAsync, GetByEventAsync
- `IDropProductRepository` — GetByEventAsync
- `IDropCouponRepository` — GetByCodeAsync
- `IDropRegistrationRepository` — GetByEventAsync, GetByCustomerAndEventAsync
- `IQueueEntryRepository` — GetByEventAsync, GetByCustomerAndEventAsync

**Store:**
- `IProductRepository` — GetBySlugAsync, GetByCategoryAsync
- `IOrderRepository` — GetByCustomerAsync, GetByStatusAsync
- `ICustomerRepository` — GetActiveAsync
- `ICouponRepository` — GetByCodeAsync
- `ICategoryRepository` — GetBySlugAsync
- `IServiceRepository` — GetByCategoryAsync

---

## Unit of Work

Encapsula o `SaveChangesAsync` do DbContext. A Application chama `IUnitOfWork.CommitAsync()` para persistir todas as mudanças pendentes em uma única transação.

---

## EntityTypeConfigurations

Cada entidade possui uma classe `IEntityTypeConfiguration<T>` que define:
- Nome da tabela (`ToTable`)
- Chave primária com auto-increment
- Tipos de coluna (`HasMaxLength`, `HasPrecision(18,2)`)
- Índices em campos frequentes (Slug, EnterpriseId, FKs)
- Relacionamentos com `OnDelete(Restrict)`
- `HasDefaultValue(false)` para `IsDeleted`

### Seed Data (StaticEntities)

As 17 StaticEntities do Drop possuem `HasData()` com os valores pré-definidos no Domain. Exemplo:

```csharp
builder.HasData(
    new { Id = 1L, Description = "Rascunho" },
    new { Id = 2L, Description = "Inscrições abertas" },
    ...
);
```

### Value Objects (Store)

Enterprise e Employee possuem Value Objects configurados como Owned Types:

```csharp
builder.OwnsOne(e => e.Email, nav => {
    nav.Property(p => p.Value).HasColumnName("Email").HasMaxLength(200).IsRequired();
});
```

---

## Convenção Snake Case

Aplicada globalmente no `OnModelCreating` de ambos os DbContexts:

```csharp
foreach (var entity in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entity.GetProperties())
        property.SetColumnName(ToSnakeCase(property.GetColumnName()!));

    foreach (var key in entity.GetKeys())
        key.SetName(ToSnakeCase(key.GetName()!));

    foreach (var fk in entity.GetForeignKeys())
        fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));

    foreach (var index in entity.GetIndexes())
        index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
}
```

Roda **uma única vez** no startup (modelo é cacheado). Zero impacto em runtime.

---

## Estrutura de Pastas

```
Infrastructure.Data/
├── Context/
│   └── *DbContext.cs
├── Configurations/
│   ├── [Entidade]/
│   │   ├── EntityConfiguration.cs
│   │   └── StatusConfiguration.cs (se houver)
│   └── ...
├── Repositories/
│   ├── Base/
│   │   ├── Repository.cs
│   │   └── UnitOfWork.cs
│   ├── [Entidade]/
│   │   └── EntityRepository.cs
│   └── ...
├── Providers/
│   └── TenantProvider.cs
├── DependencyInjection.cs
└── *.csproj
```

---

## Registro de Dependências

### Infrastructure (`AddDropInfrastructure` / `AddStoreInfrastructure`)

- `DbContext` — Scoped, MySQL via Pomelo
- `ITenantProvider` → `TenantProvider` — Scoped
- `IUnitOfWork` → `UnitOfWork` — Scoped
- `IRepository<>` → `Repository<>` — Scoped (open generic)
- Repositórios específicos — Scoped

### Application (`AddDropApplication` / `AddStoreApplication`)

- MediatR handlers (assembly scan)
- FluentValidation validators (assembly scan)
- `ValidationBehaviour<,>` — pipeline behavior

### API (`AddApiConfiguration`)

- `HttpContextAccessor` (necessário para TenantProvider)
- OpenApi
- Chama Infrastructure + Application

---

## Migrations

Para gerar migrations, execute na raiz da solution:

```bash
# Drop
dotnet ef migrations add NomeMigration --project DropCommerce.Infrastructure.Data --startup-project src/Drop/DropCommerce.Api

# Store
dotnet ef migrations add NomeMigration --project StoreCommerce.Infrastructure.Data --startup-project src/Store/StoreCommerce.Api
```

---

## SaveChangesAsync Override

Ambos os DbContexts sobrescrevem `SaveChangesAsync` para setar `UpdatedAt = DateTime.UtcNow` automaticamente em qualquer entidade modificada, sem necessidade de lógica manual nos handlers.
