# Storefront por loja

O front publica cada loja em `https://drpcommerce.example/nome-da-loja`. O
identificador persistido é `storefrontSlug`. O cadastro da loja continua
recebendo `slug` e retorna `storefrontSlug` e `storefrontUrl`.

## Contrato para o front e o Gateway

| Operação | Exemplo pelo Gateway |
| --- | --- |
| Página da loja no front | `/nome-da-loja` |
| Dados públicos da loja | `GET /api/store/api/storefront/nome-da-loja/store` |
| Busca no catálogo | `GET /api/store/api/storefront/nome-da-loja/products/search` |
| Login de cliente | `POST /api/store/api/storefront/nome-da-loja/auth/login` |
| Eventos DROP | `GET /api/drop/api/storefront/nome-da-loja/drop/events` |

O Gateway remove `/api/store` ou `/api/drop` e encaminha o restante. O Store
resolve o slug de uma loja ativa. O DROP usa a mesma resolução pela API
interna do Store. Slugs desconhecidos e lojas inativas retornam HTTP 404.
As rotas administrativas e de integração continuam sob os prefixos
existentes.

## Sessão do comprador

Use as URLs da loja atual para CSRF, cadastro, login, renovação e logout.
Guarde o access token associado ao slug em uso. O backend exige que a loja
do token corresponda à loja resolvida pelo caminho, tanto no Store quanto
no DROP.

O cookie de renovação usa o nome `__Host-DRP-StorefrontRefresh-<enterpriseId>`.
Ele mantém `HttpOnly`, `Secure`, `SameSite=Strict` e `Path=/`. Em um host
compartilhado, o Store lê e apaga apenas o cookie da loja resolvida. A
renovação também confere a loja no banco antes de rotacionar o token.
Após a troca de URLs, o comprador precisa entrar novamente para criar o
cookie da loja. [Autenticação no Store](../Store/Api/Authentication.md)

## Configuração de publicação

| Variável do Store | Valor |
| --- | --- |
| `Security__StorefrontBaseUrl` | URL HTTPS da raiz pública do front, por exemplo `https://drpcommerce.example`. O Store acrescenta o slug ao gerar a URL da loja e links de identidade do cliente. |
| `Security__StorefrontApiBaseUrl` | Base pública do Store para gerar URLs de mídia. Use `/api/store` quando o front encaminhar a API no mesmo host. Em outra origem do mesmo site, informe a URL completa, por exemplo `https://api.drpcommerce.example/api/store`. |
| `MercadoPago__PaymentReturnUrl` | Página de retorno do front. Pode conter `{storeSlug}`, por exemplo `https://drpcommerce.example/{storeSlug}/pagamento`. O retorno inclui o slug e o identificador do pedido. |

Publique front e API no mesmo site HTTPS para os cookies do storefront.
Se forem origens diferentes no mesmo site, configure a origem do front em
`Security__AllowedOrigins` e envie credenciais nas chamadas de autenticação.

Aplique a migração `AddEnterpriseStorefrontSlug` no banco do Store antes de
atualizar Store e DROP em conjunto. As lojas já cadastradas recebem um caminho
`loja-<id>`. Novas lojas usam o slug escolhido no cadastro. Um índice único
garante um identificador por loja.
