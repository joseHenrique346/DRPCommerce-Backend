# Melhor Envio

O Store autoriza a conta do Melhor Envio por loja e ambiente. O pedido de
consentimento usa os escopos `users-read` (identificar a conta) e
`shipping-calculate` (cotação de frete). O callback grava a identidade da conta
consultada com o token e protege as credenciais no servidor.
[Escopos e fluxo de autorização](https://docs.melhorenvio.com.br/reference/fluxo-de-autoriza%C3%A7%C3%A3o)

## Configuração de sandbox

Cadastre um aplicativo no painel do sandbox do Melhor Envio. Configure a URL de
callback idêntica à que o Store enviará no parâmetro `redirect_uri`. O início do
OAuth e o callback precisam usar o mesmo host HTTPS público, pois o cookie de
vínculo pertence a esse host. Caso o Store seja publicado pelo Gateway, inclua
`/api/store` na base pública.
[Ambientes do Melhor Envio](https://docs.melhorenvio.com.br/reference/introducao-api-melhor-envio)

| Configuração do Store | Valor |
| --- | --- |
| `MelhorEnvio__PublicApiBaseUrl` | Base HTTPS pública que encaminha para o Store. |
| `MelhorEnvio__BackofficeReturnUrl` | URL HTTPS do backoffice para o retorno após a conexão. |
| `MelhorEnvio__ContactEmail` | E-mail técnico enviado no `User-Agent`. |
| `MelhorEnvio__Sandbox__ClientId` | Identificador numérico do aplicativo de sandbox. |
| `MelhorEnvio__Sandbox__ClientSecret` | Segredo do aplicativo, injetado pelo armazenamento de segredos do servidor. |
| `MelhorEnvio__Sandbox__Enabled` | `true` para habilitar o sandbox. |
| `ProviderIntegrations__KeyRingPath` | Caminho persistente do chaveiro do Store. |

O callback cadastrado é a base pública seguida de
`/api/integrations/melhor-envio/oauth/sandbox/callback`. A produção usa a seção
`MelhorEnvio__Production__`, um aplicativo próprio e o caminho
`/api/integrations/melhor-envio/oauth/production/callback`. Os dois ambientes
começam desabilitados.

## Fluxo

1. Aplique a migração do Store `BindProviderAuthorizationToProvider` no ambiente
   de teste. Ela identifica os estados existentes como Mercado Pago e preserva
   o ambiente já gravado em cada registro.
2. No navegador autenticado com `settings.manage`, faça
   `POST /api/integrations/melhor-envio/oauth/start` no Store com
   `{"environment":1}` (`Sandbox`). Para iniciar pelo Swagger, abra-o no
   mesmo host público do callback. Se a chamada partir de outra origem do
   mesmo site, habilite o envio de credenciais no navegador e configure essa
   origem no CORS do Store. O callback depende do cookie HTTPS recebido nessa
   chamada.
3. Navegue para a `authorizationUrl` retornada. Após o consentimento, o callback
   valida provedor, ambiente, navegador, validade e uso único do `state`.
   A permissão da loja é consultada antes da troca do código e novamente antes
   da gravação. Em caso de sucesso, o navegador retorna ao backoffice com
   `melhorEnvio=connected`.
4. Use `GET /api/provider-connections` para consultar os metadados da conexão.
   `DELETE /api/provider-connections/melhor-envio/Sandbox` desconecta localmente.

A renovação reutiliza a lease de credenciais do Store para evitar trocas
simultâneas. Cada renovação confirma a identidade da conta antes de salvar os
tokens rotacionados.
[Troca e renovação do token](https://docs.melhorenvio.com.br/reference/solicitacao-do-token)
[Identidade da conta](https://docs.melhorenvio.com.br/reference/listar-informacoes-do-usuario)

## Próxima etapa

A próxima validação externa é autorizar uma loja com o aplicativo de sandbox.

- A cotação precisa de origem e destino estruturados, peso em kg e dimensões
  em cm por SKU. No checkout, confira a escolha contra a loja, o carrinho,
  o endereço e a validade da cotação. [Documentação de cotação](https://docs.melhorenvio.com.br/docs/cotacao-de-fretes)
- Grave o serviço, o preço e os volumes no pedido antes de decidir se haverá
  tentativa de pagamento. Inclua nos testes pedidos com desconto integral nos
  produtos e os fluxos de retirada e de produtos digitais.
- Separe compra e geração da etiqueta de postagem e entrega. O vínculo por
  volume precisa preservar os IDs externos para reconciliar falhas de
  comunicação. [Dados exigidos para etiquetas](https://docs.melhorenvio.com.br/reference/inserir-fretes-no-carrinho)
- Defina qual módulo executa a logística das vendas DROP. Reutilize em Shared
  os contratos de logística usados pelos dois módulos e preserve a propriedade
  de cada pedido e saldo de estoque.
