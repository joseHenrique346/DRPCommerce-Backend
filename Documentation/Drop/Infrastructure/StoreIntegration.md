# Integração Store → Drop com Refit

## Objetivo

O DropCommerce consulta dados canônicos de produtos e empresas mantidos pelo StoreCommerce. A comunicação é feita por HTTP usando Refit, sem compartilhar entidades, DbContexts ou repositórios entre os módulos.

Cada módulo continua dono do seu banco de dados. O Drop mantém apenas os dados próprios e usa os identificadores recebidos do Store; não existe FK entre os bancos.

## Responsabilidade de cada camada

### Application do Drop

As interfaces abaixo representam a necessidade do caso de uso, sem referência a HTTP ou Refit:

- `IStoreProductReader`: consulta um produto por `ProductId` e valida o `EnterpriseId`.
- `IStoreEnterpriseReader`: consulta uma empresa por `EnterpriseId`.

Os contratos retornados são `StoreProductData` e `StoreEnterpriseData`.

### Infrastructure do Drop

- `IStoreProductRefitApi` e `IStoreEnterpriseRefitApi` descrevem as chamadas HTTP.
- `StoreProductResponse` e `StoreEnterpriseResponse` são DTOs exclusivos da integração.
- `StoreProductReader` e `StoreEnterpriseReader` adaptam os DTOs HTTP para os contratos da Application.
- `StoreIntegrationResponseReader` centraliza a leitura do envelope `Result` retornado pelo Store.
- `StoreIntegrationException` representa falhas de comunicação ou respostas inválidas.

Essa separação permite que a Application conheça apenas readers e contratos de negócio, enquanto Refit fica restrito à Infrastructure.

## Fluxo de chamada

```text
Handler do Drop
    ↓
IStoreProductReader / IStoreEnterpriseReader
    ↓
StoreProductReader / StoreEnterpriseReader
    ↓
IStoreProductRefitApi / IStoreEnterpriseRefitApi
    ↓
HttpClientFactory + Refit
    ↓
StoreCommerce.Api
```

## Endpoints consumidos

| Recurso | Método | Endpoint | Retorno da Application |
|---|---|---|---|
| Produto | `GET` | `/internal/v1/drop/products/{productId}?enterpriseId={enterpriseId}` | `StoreProductData?` |
| Empresa | `GET` | `/internal/v1/drop/enterprises/{enterpriseId}` | `StoreEnterpriseData?` |

O prefixo `/internal/v1/drop` identifica endpoints destinados à comunicação entre os módulos. A implementação correspondente deve existir no StoreCommerce.Api.

## Envelope HTTP

Os endpoints utilizam o envelope de resultado do Store:

```json
{
  "isSuccess": true,
  "content": {
    "productId": 10,
    "enterpriseId": 2,
    "name": "Produto exemplo",
    "sku": "SKU-10",
    "isActive": true
  },
  "listMessageErrors": []
}
```

O adapter converte somente os campos necessários:

| DTO HTTP | Contrato da Application |
|---|---|
| `productId` | `StoreProductData.ProductId` |
| `enterpriseId` | `StoreProductData.EnterpriseId` / `StoreEnterpriseData.EnterpriseId` |
| `name` | `StoreProductData.Name` |
| `sku` | `StoreProductData.SKU` |
| `tradeName` | `StoreEnterpriseData.TradeName` |
| `isActive` | `StoreProductData.IsActive` / `StoreEnterpriseData.IsActive` |

Se o produto retornado pertencer a outro `EnterpriseId`, o reader retorna `null` para evitar o uso de dados de outro tenant.

## Configuração

No `DropCommerce.Api`, configure a URL do Store:

```json
{
  "StoreApi": {
    "BaseUrl": "http://localhost:5001"
  }
}
```

Em ambiente de produção, a variável `STORECOMMERCE_API_BASE_URL` pode sobrescrever o valor do arquivo de configuração.

O registro dos clients acontece em `AddDropInfrastructure` por meio de `AddRefitClient`. A URL não fica fixa no código dos clients.

## Timeout e resiliência

As consultas são configuradas com `Microsoft.Extensions.Http.Resilience`:

- até 2 segundos por tentativa;
- até 5 segundos para a operação completa, incluindo retries;
- até 2 retries para falhas transitórias;
- retries desabilitados para métodos HTTP inseguros; esta integração possui apenas consultas `GET`;
- circuit breaker para impedir chamadas repetidas quando o Store está indisponível.

O `HttpClient` usa timeout infinito porque o timeout efetivo é controlado pela política de resiliência. O `CancellationToken` recebido pelo reader é encaminhado ao Refit.

## Tratamento de respostas

- `404`: o reader retorna `null`, indicando que o recurso não foi encontrado.
- `401`, `403`, `5xx` e demais status HTTP não bem-sucedidos: lançam `StoreIntegrationException`.
- Resposta sem conteúdo ou com falha de desserialização: lançam `StoreIntegrationException`.
- `IsSuccess = false` no envelope: a mensagem do Store é preservada na exceção.
- Timeout da política ou da requisição: é convertido em erro de integração.
- Cancelamento solicitado pelo consumidor: o cancelamento continua sendo propagado.

## Execução local

1. Inicie o StoreCommerce.Api na porta `5001`.
2. Inicie o DropCommerce.Api na porta `5002`.
3. Confirme `StoreApi:BaseUrl` no appsettings do Drop.
4. Use os casos de uso do Drop que dependem de `IStoreProductReader` ou `IStoreEnterpriseReader`.
