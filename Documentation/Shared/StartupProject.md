# Startup do Projeto DROP-Ecommerce

Revisado em 03/11/2026. Execute os comandos a partir da raiz do repositório. Esta atualização documental não cria bancos nem aplica migrations.

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL acessível pelas APIs (porta padrão 5432)
- CLI `dotnet-ef` compatível com EF Core 9 para administrar migrations
- IDE recomendada: Visual Studio 2022+ ou Rider

## Estrutura de Serviços

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| Commerce.Gateway | 5000 | API Gateway (YARP) — ponto de entrada único |
| StoreCommerce.Api | 5001 | API de loja |
| DropCommerce.Api | 5002 | API de drop commerce |

## Configuração do Banco de Dados

Cada API possui seu próprio banco PostgreSQL. Crie bancos/usuário no ambiente escolhido e copie os templates somente se ainda não houver configuração local:

```powershell
Copy-Item DROP-Ecommerce/src/Drop/DropCommerce.Api/appsettings.Development.Template.json DROP-Ecommerce/src/Drop/DropCommerce.Api/appsettings.Development.json

Copy-Item DROP-Ecommerce/src/Store/StoreCommerce.Api/appsettings.Development.Template.json DROP-Ecommerce/src/Store/StoreCommerce.Api/appsettings.Development.json
```

Preencha com as credenciais PostgreSQL de cada banco. Exemplo ilustrativo do Store:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=drpcommerce_store;Username=<USER>;Password=<PASSWORD>;"
  }
}
```

Ou configure via variáveis de ambiente:

- `DROPCOMMERCE_CONNECTION_STRING`
- `STORECOMMERCE_CONNECTION_STRING`

Essas variáveis têm prioridade sobre `ConnectionStrings:DefaultConnection`. O template do Drop sugere `drpcommerce_drop`; os módulos usam bancos separados. Configuração vazia impede inicialização. Os arquivos locais com credenciais não devem ser versionados.

### Aplicar Migrations

Primeiro confira a configuração e as migrations de cada módulo:

```powershell
dotnet ef migrations list --project DROP-Ecommerce/StoreCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Store/StoreCommerce.Api
dotnet ef migrations list --project DROP-Ecommerce/DropCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Drop/DropCommerce.Api
```

Para aplicar ao ambiente escolhido:

```powershell
dotnet ef database update --project DROP-Ecommerce/StoreCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Store/StoreCommerce.Api
dotnet ef database update --project DROP-Ecommerce/DropCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Drop/DropCommerce.Api
```

As APIs não aplicam migrations automaticamente. Bases legadas podem exigir importação antes de `IdentityAndSessions`, correção de duplicidades de slug/SKU e ajustes comerciais posteriores. `ReserveCouponUsesAtCheckout` compromete usos de cupons dos pedidos pendentes existentes. Veja [infraestrutura](Infrastructure.md) e as notas de [identidade](../Store/Domain/Identity.md), [estoque](../Store/Application/ProductsAndInventory.md) e [cupons](../Store/Application/Coupons.md).

## Segurança e integração obrigatórias

Configure nos arquivos privados, user-secrets ou ambiente de cada processo:

| Configuração | Uso |
| --- | --- |
| `Security__SigningKey` | Chave aleatória com pelo menos 32 bytes; igual em Store e Drop. |
| `Security__InternalServiceKey` | Segredo interno distinto, com pelo menos 32 caracteres; igual nos dois serviços para integração. |
| `Security__Issuer` | Mesmo emissor nas APIs; padrão DRPCommerce. |
| `STORECOMMERCE_API_BASE_URL` | URL do Store vista pelo Drop; perfil HTTPS define https://localhost:5001. |
| `Security__StorefrontBaseDomain` | Base dos domínios das lojas; template usa stores.localhost. |
| `Security__AllowedOrigins__0`, etc. | Origins exatas do frontend quando houver CORS. |
| `Security__TrustedProxies__0`, etc. | IPs de proxies confiáveis para forwarding. |

Variáveis de um terminal são herdadas pelos processos iniciados ali; terminais independentes precisam receber sua configuração. Não inserir segredos reais nos templates. Detalhes de JWT, cookies e CSRF: [Authentication](../Store/Api/Authentication.md).

O cadastro da loja cria o registro de subdomínio, mas não configura DNS/hosts nem certificado HTTPS. A vitrine exige domínio ativo/verificado da loja. Envio de e-mail continua desabilitado; `RequireConfirmedEmail=false` permite o recorte de desenvolvimento sem comprovar titularidade do e-mail.

## Executando

### Via CLI (em terminais separados)

```bash
dotnet run --project DROP-Ecommerce/src/Store/StoreCommerce.Api --launch-profile https
dotnet run --project DROP-Ecommerce/src/Drop/DropCommerce.Api --launch-profile https
dotnet run --project DROP-Ecommerce/src/Gateway/Commerce.Gateway --launch-profile https
```

Inicie Store antes de usar operações protegidas do Drop: ele consulta o Store para validar sessão/tenant. Para conferir compilação sem instalar hooks:

```powershell
dotnet build DROP-Ecommerce/DROP-Ecommerce.sln -p:InstallGitHooks=false
```

### Via Visual Studio

1. Abra `DROP-Ecommerce/DROP-Ecommerce.sln`
2. Selecione o perfil de inicialização `HttpsDRP-Commerce` para testar autenticação.
3. Os três projetos devem ter `https` como destino de depuração, não `IIS Express`.
4. F5. No perfil HTTPS, somente o Gateway abre uma aba; acesse os Swaggers pelos caminhos abaixo.

O perfil `https` usa Kestrel com portas fixas: Gateway em `https://localhost:5000`, Store em `https://localhost:5001` e Drop em `https://localhost:5002`. Nesse perfil, o Gateway encaminha para as APIs por HTTPS e o Drop consulta o Store em `https://localhost:5001`. Esses endereços são definidos nos perfis locais; as rotas e os destinos HTTP padrão do Gateway permanecem iguais.

Ao iniciar `HttpsDRP-Commerce`, os três projetos continuam executando, mas somente o Gateway abre uma aba no navegador. Os perfis `https` do Store e do Drop usam `launchBrowser: false`; os Swaggers dessas APIs continuam disponíveis para acesso manual.

O perfil `DRP-Commerce` seleciona explicitamente o destino `http` para manter a execução HTTP anterior nas mesmas portas. Execute apenas um dos dois perfis por vez, pois usam as mesmas portas. Para autenticação com cookies Secure e CSRF, use o perfil HTTPS.

Os perfis `IIS Express` foram preservados, mas usam as portas próprias de `iisSettings`, não as URLs dos perfis de projeto.

Se necessário, configure a confiança no certificado de desenvolvimento antes de iniciar:

```powershell
dotnet dev-certs https --trust
```

Não desabilite a validação TLS dos clients internos. Na CLI, acrescente `--launch-profile https` a cada comando `dotnet run` para usar os mesmos perfis HTTPS.

## Rotas (via Gateway)

O ponto de entrada para o frontend é o Gateway na porta **5000**. Chamadas internas de serviço usam o destino privado configurado:

| Rota | Destino |
|------|---------|
| `http://localhost:5000/api/store/{endpoint}` | StoreCommerce.Api |
| `http://localhost:5000/api/drop/{endpoint}` | DropCommerce.Api |

O Gateway remove o prefixo antes de encaminhar. Exemplo real no perfil HTTPS: `GET https://localhost:5000/api/store/api/storefront/products/search` chega ao Store como `/api/storefront/products/search`.

No perfil HTTPS, troque somente o protocolo por `https`. Como o prefixo é removido, uma rota interna `/api/auth/csrf` do Store é acessada por `https://localhost:5000/api/store/api/auth/csrf`.

## Swagger

Acesse diretamente em cada serviço (apenas em Development):

- **Drop API:** http://localhost:5002/swagger
- **Store API:** http://localhost:5001/swagger

No perfil `HttpsDRP-Commerce`:

- **Drop API:** https://localhost:5002/swagger
- **Store API:** https://localhost:5001/swagger

Também é possível usar os Swaggers pelo Gateway:

- **Store API:** https://localhost:5000/api/store/swagger/index.html
- **Drop API:** https://localhost:5000/api/drop/swagger/index.html

O documento Swagger declara a URL base relativa `../..`, resolvida a partir de `/swagger/v1/swagger.json`. Assim, o botão `Execute` preserva `/api/store` ou `/api/drop` quando aberto pelo Gateway, e continua funcionando no acesso direto às APIs. As rotas dos controllers não mudam.

Por exemplo, o GET `/api/auth/csrf` deve mostrar `https://localhost:5000/api/store/api/auth/csrf` em `Request URL` no Swagger do Store aberto pelo Gateway. Depois de alterar essa configuração, reinicie as APIs e recarregue a página do Swagger para obter o documento atualizado.

O Gateway não possui Swagger próprio; os caminhos acima encaminham para a documentação de cada API.

O catch-all do Gateway não estabelece sozinho uma fronteira privada para `/api/internal`. Ao publicar, bloquear esse prefixo na borda e configurar forwarding/Host confiável. A chave interna permanece no backend.

## Fluxos comerciais disponíveis

O Store já possui catálogo com variantes, estoque, cupons, carrinho persistido, checkout, pedidos numerados, histórico e endereços. Pagamento/estorno são manuais, frete permanece zerado e não há integração fiscal. No Drop, entrada na fila e consultas existem, mas o ciclo completo de admissão/reserva/venda continua pendente.

- [Referência dos endpoints do Store](../Store/Api/Endpoints.md)
- [Carrinho, checkout e pedidos](../Store/Application/OrdersAndCheckout.md)
- [Integração Store/Drop](../Drop/Infrastructure/StoreIntegration.md)
- [Integrações externas pendentes](PlanejamentoIntegracoes.md)
