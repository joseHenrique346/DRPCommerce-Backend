# Startup do Projeto DROP-Ecommerce

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL](https://dev.mysql.com/downloads/) (porta padrão 3306)
- IDE recomendada: Visual Studio 2022+ ou Rider

## Estrutura de Serviços

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| Commerce.Gateway | 5000 | API Gateway (YARP) — ponto de entrada único |
| StoreCommerce.Api | 5001 | API de loja |
| DropCommerce.Api | 5002 | API de drop commerce |

## Configuração do Banco de Dados

Cada API possui seu próprio banco MySQL. Copie os templates:

```bash
cp DROP-Ecommerce/src/Drop/DropCommerce.Api/appsettings.Development.Template.json DROP-Ecommerce/src/Drop/DropCommerce.Api/appsettings.Development.json

cp DROP-Ecommerce/src/Store/StoreCommerce.Api/appsettings.Development.Template.json DROP-Ecommerce/src/Store/StoreCommerce.Api/appsettings.Development.json
```

Preencha com suas credenciais MySQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=DropCommerceDb;User=root;Password=sua_senha;"
  }
}
```

Ou configure via variáveis de ambiente:

- `DROPCOMMERCE_CONNECTION_STRING`
- `STORECOMMERCE_CONNECTION_STRING`

### Aplicar Migrations

```bash
dotnet ef database update --project DropCommerce.Infrastructure.Data --startup-project src/Drop/DropCommerce.Api
```

## Executando

### Via CLI (em terminais separados)

```bash
dotnet run --project DROP-Ecommerce/src/Gateway/Commerce.Gateway
dotnet run --project DROP-Ecommerce/src/Store/StoreCommerce.Api
dotnet run --project DROP-Ecommerce/src/Drop/DropCommerce.Api
```

### Via Visual Studio

1. Abra `DROP-Ecommerce/DROP-Ecommerce.sln`
2. Configure múltiplos startup projects (Commerce.Gateway, StoreCommerce.Api, DropCommerce.Api)
3. F5

## Rotas (via Gateway)

Todas as requisições devem passar pelo Gateway na porta **5000**:

| Rota | Destino |
|------|---------|
| `http://localhost:5000/api/store/{endpoint}` | StoreCommerce.Api |
| `http://localhost:5000/api/drop/{endpoint}` | DropCommerce.Api |

O Gateway remove o prefixo antes de encaminhar. Exemplo: `GET http://localhost:5000/api/drop/products` chega na Drop API como `/products`.

## Swagger

Acesse diretamente em cada serviço (apenas em Development):

- **Drop API:** http://localhost:5002/swagger
- **Store API:** http://localhost:5001/swagger
