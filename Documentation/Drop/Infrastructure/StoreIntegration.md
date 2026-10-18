# Integração Store / Drop

O Store é fonte canônica de identidade, clientes, funcionários, produtos, lojas e memberships. O Drop mantém banco próprio; IDs externos não são FKs entre bancos.

## Validação de sessão

```text
Requisição ao Drop
  → valida assinatura, emissor, audience e validade do JWT
  → chama Store /api/internal/identity/validate com chave interna
  → Store confere conta, família de sessão, versões, membership ou Customer e domínio
  → Drop aplica tenant, propriedade do recurso e permissão operacional
```

Não há cache de sessão nesta etapa. Revogações e alterações de acesso são observadas na próxima requisição protegida. Se o Store não puder ser consultado, o Drop falha fechado; a operação não é liberada.

O storefront resolve o Host consultando `/api/internal/tenants/resolve`. Domínio deve estar verificado/ativo e a loja ativa. CustomerId vem do JWT e não é um identificador selecionável pelo comprador.

## Endpoints internos do Store

| Método | Endpoint | Contrato |
|---|---|---|
| POST | /api/internal/identity/validate | accessToken, host → isValid |
| GET | /api/internal/tenants/resolve?host=... | enterpriseId |
| GET | /api/internal/products/get-by-id/{id} | Produto canônico |
| POST | /api/internal/products/get-list-by-list-id | Lista de IDs |
| GET | /api/internal/customers/get-by-id/{id} | Cliente canônico |
| POST | /api/internal/customers/get-list-by-list-id | Lista de IDs |
| GET | /api/internal/employees/get-by-id/{id} | Funcionário canônico |
| POST | /api/internal/employees/get-list-by-list-id | Lista de IDs |
| GET | /api/internal/enterprises/get-by-id/{id} | Loja canônica |

Todas exigem X-Service-Key. Recursos exigem contexto X-Enterprise-Id válido, colocado pelo serviço confiável. Consultas de recursos preservam o envelope Result do Store; validação de identidade e resolução de tenant usam DTOs diretos.

Essas rotas não substituem autenticação pública com um header de tenant. Não publique o prefixo interno no Gateway aberto à internet.

## Camadas e clients

Application define IStoreProductReader, IStoreEnterpriseReader, IStoreCustomerReader e IStoreEmployeeReader, com contratos próprios, sem depender de Refit.

Infrastructure contém os clients Refit, DTOs HTTP, adapters e StoreIntegrationResponseReader. DropCommerce.Security contém StoreServiceAuthenticationHandler, que injeta chave e tenant autenticado; lê o contexto da requisição por IHttpContextAccessor, evitando dependência de escopo incorreto no HttpClientFactory. Os clients dependem do contrato abstrato StoreAuthenticationHandler em Application, sem referenciar o projeto Security.

StoreSessionValidator, DropTenantResolver e a implementação de IDropOperationScope também pertencem a DropCommerce.Security. A composição é feita pela API por AddDropSecurity. Veja [Organização dos projetos Security](../../Shared/Security.md).

A versão Refit 14 instalada usa AddRefitGeneratedClient e SystemTextJsonContentSerializer explícito. Foram preservadas as interfaces/adapters existentes; não foi acrescentado Refit.Reflection.

Consultas de recursos usam até 2 segundos por tentativa, 5 segundos totais e até 2 retries, desabilitados para POST. Há circuit breaker. Clientes de sessão/resolução têm timeout de 5 segundos. Cancelamento do chamador continua sendo propagado.

## Configuração

Configure no Drop:

- STORECOMMERCE_API_BASE_URL: URL interna do Store.
- Security__InternalServiceKey: mesmo segredo configurado no Store.
- Security__SigningKey e Security__Issuer: iguais nos dois serviços.
- Bancos separados via STORECOMMERCE_CONNECTION_STRING e DROPCOMMERCE_CONNECTION_STRING.
- TrustedProxies: IPs explícitos do proxy, quando houver forwarding.

A URL não fica fixa no client. Use HTTPS ou uma rede interna privada protegida; não remova validação TLS para contornar certificados inválidos.

## Isolamento e falhas

- Readers rejeitam recurso canônico que não corresponde ao tenant esperado.
- Proteções de escrita do Drop validam eventos, clientes/produtos canônicos e relações do mesmo tenant/evento/cliente.
- Query filters protegem dependentes; quando o ator é Customer, recursos privados também exigem seu CustomerId.
- Token de fila, sessão e lock de reserva não aparecem em respostas JSON.
- Identidade inválida/revogada resulta em rejeição de autenticação.
- Store indisponível ou resposta de integração inválida não concede acesso; serviços/controladores retornam falha.
- Audit logs não têm escrita manual pública.

A validação integrada foi executada com bancos temporários e dados fictícios; o script temporário foi removido após a validação. Detalhes de configuração, endpoints do frontend e limites de produção: [Autenticação e segurança](../../Store/Api/Authentication.md).

Referência da biblioteca: [Refit](https://github.com/reactiveui/refit).
