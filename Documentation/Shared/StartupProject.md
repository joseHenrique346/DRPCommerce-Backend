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

Todas as requisições devem passar pelo Gateway na porta **5000**:

| Rota | Destino |
|------|---------|
| `http://localhost:5000/api/store/{endpoint}` | StoreCommerce.Api |
| `http://localhost:5000/api/drop/{endpoint}` | DropCommerce.Api |

O Gateway remove o prefixo antes de encaminhar. Exemplo: `GET http://localhost:5000/api/drop/products` chega na Drop API como `/products`.

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
