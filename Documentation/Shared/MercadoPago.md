# Mercado Pago no backend

O Store implementa Checkout Pro via Preferences API, OAuth por loja, renovação de tokens, notificações assinadas, consulta periódica, cancelamento e estorno integral. A integração fica **desabilitada por padrão**. A seleção é persistida por loja/ambiente; lojas sem seleção usam manual. Desconectar Mercado Pago não muda silenciosamente os novos checkouts para manual.

## Aplicações preparadas pelo MCP

| Recurso | ID |
| --- | --- |
| DRPCommerceDev, aplicação principal | `1280353562761045` |
| Aplicação do vendedor de teste | `3054928538483294` |
| Vendedor de teste | `3857410022` |
| Comprador de teste | `3853433813` |

Esses IDs não são credenciais. A aplicação pertence à conta autenticada no MCP. As credenciais produtivas ainda não foram ativadas nesta preparação. O login do MCP não conecta automaticamente uma loja ao backend.

## Configuração do ambiente

Use .NET User Secrets em Development ou o armazenamento de segredos do hospedeiro. O projeto da API tem `UserSecretsId`. Chaves não devem ser colocadas no `appsettings.json` versionado.

| Variável de ambiente | Finalidade |
| --- | --- |
| `MercadoPago__Enabled` | Habilita a integração; padrão `false`. |
| `MercadoPago__AllowProduction` | Permite produção; padrão `false`, manter assim durante os testes. |
| `MercadoPago__ClientId` | Aplicação do OAuth. O exemplo versionado contém o ID da DRPCommerceDev. |
| `MercadoPago__ClientSecret` | Segredo OAuth dessa aplicação. |
| `MercadoPago__PublicApiBaseUrl` | Base HTTPS pública da API, com eventual PathBase e sem acrescentar `/api`. |
| `MercadoPago__BackofficeReturnUrl` | Página HTTPS administrativa para o retorno do OAuth. |
| `MercadoPago__PaymentReturnUrl` | Página HTTPS da vitrine após o checkout; o backend acrescenta `orderId`. |
| `MercadoPago__SandboxAccessToken` | Token do vendedor de teste, usado pelo endpoint de conexão do sandbox. |
| `MercadoPago__SandboxWebhookSecret` | Assinatura da aplicação que emite os pagamentos de teste. Na automação do MCP, é a aplicação do vendedor de teste. |
| `MercadoPago__WebhookSecret` | Assinatura da aplicação produtiva, separada do sandbox. |
| `MercadoPago__StatementDescriptor` | Descritor de fatura, de 1 a 22 caracteres. |
| `MercadoPago__SignatureToleranceSeconds` | Tolerância da assinatura; padrão 600 segundos. |
| `ProviderIntegrations__KeyRingPath` | Diretório persistente das chaves de proteção de tokens e PKCE. |
| `ProviderIntegrations__KeyEncryptionCertificateThumbprint` | Certificado do chaveiro quando necessário; Windows usa DPAPI quando não há certificado. |

O ambiente não é inferido pelo prefixo da chave. O adapter consulta `/users/me`, verifica a identidade e exige conta brasileira de teste no sandbox. Produção exige conta real e pagamentos produtivos. [Modelo de contas de teste](https://www.mercadopago.com.br/developers/pt/docs/checkout-pro-preferences/test-accounts).

Para o painel em outra origem, configurar também `Security:AllowedOrigins` e permitir o envio de credenciais conforme a política CORS existente. Usar HTTPS e encaminhamento de protocolo apenas por proxies confiáveis, para preservar o cookie de vínculo do OAuth.

Aplicar as migrações `AddMercadoPagoOAuthAndNotifications` e `SupportMercadoPagoBalance` antes de habilitar a nova versão. Elas criam autorizações, seleção, caixa de entrada, agendamento de consultas, vínculos de captura e o meio de pagamento Saldo Mercado Pago. Não conectam lojas nem ativam produção.

```powershell
dotnet ef database update --project DROP-Ecommerce/StoreCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Store/StoreCommerce.Api --context StoreCommerceDbContext
```

O rollback recusa descartar estrutura com seleção financeira, notificações ou capturas vinculadas. Correções posteriores devem preservar esse histórico.

## Endpoints e frontend

Administração exige usuário com loja selecionada e `settings.manage`. Callback e webhook são públicos, com validações próprias. Respostas não expõem tokens.

| Método e rota | Uso |
| --- | --- |
| `GET /api/integrations/mercado-pago` | Status, seleção, conexões e URL de webhook de cada conexão. |
| `POST /api/integrations/mercado-pago/oauth/start` | `{ "environment": 1 }` para sandbox ou `2` para produção. Retorna `authorizationUrl` e `expiresAt`; grava cookie do navegador. |
| `GET /api/integrations/mercado-pago/oauth/callback` | Valida state/cookie/PKCE, troca o código e conecta a conta. |
| `POST /api/integrations/mercado-pago/sandbox/connect` | Conecta o vendedor de teste pré-configurado no servidor. Não recebe token no body. |
| `PUT /api/integrations/mercado-pago/selection` | `{ "provider": "mercado-pago", "environment": 1 }` ou `{ "provider": "manual", "environment": 1 }`. |
| `GET /api/integrations/mercado-pago/notifications` | Últimas 100 notificações pendentes/em revisão da loja. |
| `POST /api/integrations/mercado-pago/webhooks/{connectionId}` | Tópico `payment`, query `data.id`, mesmo ID no body, headers `x-signature` e `x-request-id`. |
| `DELETE /api/provider-connections/mercado-pago/{environment}` | Desconexão local; apaga tokens e preserva histórico/seleção. |

Fluxo OAuth:

1. Registrar `{PublicApiBaseUrl}/api/integrations/mercado-pago/oauth/callback` exatamente na aplicação e habilitar PKCE no provedor.
2. O frontend chama `oauth/start` com Bearer e `credentials: "include"`, para armazenar o cookie HttpOnly, e navega para a URL retornada.
3. O vendedor autoriza sua própria conta. O callback confere state aleatório/hash, cookie do navegador, prazo de dez minutos, uso único e PKCE S256.
4. Revalida usuário ativo, versão de segurança, vínculo e permissão atual. Troca o código fora da transação, consulta o recebedor e grava tokens protegidos/conexão/seleção atomicamente.
5. Redireciona somente para `BackofficeReturnUrl`, com `mercadoPago=connected`. O frontend confirma o resultado pelo status autenticado.

No sandbox do MCP, pode-se configurar o token do vendedor de teste no servidor e usar `sandbox/connect`. A identidade remota é conferida e uma conta real é recusada. A chave estática recebe validade **local** de uma hora, sem inventar validade remota ou refresh token; reconectar renova essa janela. Para OAuth interativo são necessárias as credenciais OAuth e o callback registrado na aplicação correspondente. [OAuth e PKCE](https://www.mercadopago.com.br/developers/pt/docs/security/oauth/creation).

A vitrine mantém o checkout existente, abre `payment.checkoutUrl` e consulta o detalhe autenticado do pedido após o retorno. Query strings do navegador não confirmam pagamento. `payment.attempt` e `paymentOperations` continuam disponíveis.

## Comportamento financeiro

A preferência envia valor/moeda congelados, referência opaca, comprador, vencimento, descritor e URLs. Representa o total do pedido como um item agregado. Só envia um POST: resposta perdida passa à busca por `external_reference`, validando conta, total e moeda. Busca vazia não autoriza outro POST, e busca ambígua não escolhe arbitrariamente um resultado. [Busca de preferências](https://www.mercadopago.com.br/developers/en/reference/online-payments/checkout-pro-preferences/search-preferences/get).

O Checkout Pro não permite excluir `account_money`: essa exclusão foi recusada com HTTP 400 na validação com o vendedor de teste. O adapter mantém saldo em conta disponível e registra pagamentos verificados desse tipo como `Saldo Mercado Pago` (TransactionMethod 4), sem alterar o método solicitado originalmente na tentativa.

Falha anterior à escrita é diferente de resposta perdida: criação comprovadamente não enviada volta a `Requested` ou é abortada sob lock se o pedido expirou/cancelou. Estorno não enviado fica em falha e admite retry explícito com a mesma chave. Operações já enviadas e incertas continuam sendo recuperadas somente por consulta.

O cancelamento expira a preferência e cancela pagamentos pendentes/autorizados da correlação. Recuperação não repete PUT/POST. Aprovação tardia mantém o pedido cancelado e solicita estorno. PaymentId já verificado permite estorno mesmo enquanto a resposta da preferência está em recuperação.

Estorno integral verifica pagamento, recebedor, valor, moeda e reembolsos anteriores e usa `X-Idempotency-Key` estável. Envia o POST sem corpo: o campo `amount` é reservado ao contrato de reembolso parcial. Só confirma o lançamento com resultado financeiro comprovado. Resposta perdida é recuperada por consulta do pagamento integralmente reembolsado. [API de estornos](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-pro-preferences/create-refund/post).

A implementação atual usa corpo HTTP ausente. O SDK oficial também representa o integral com JSON `{}` e `Content-Type: application/json`; a exigência comum é omitir `amount`. Uma comparação diagnóstica com JSON `{}` explícito e a mesma chave da operação também recebeu HTTP 401, causa 7. O resultado, os headers de teste e a autorização foram detalhados no [diagnóstico de estorno no sandbox](DiagnosticoEstornoSandbox.md); a diferença de formato não resolveu a recusa.

## Webhook e recuperação

O endpoint valida HMAC SHA-256 do manifesto `id:{data.id};request-id:{x-request-id};ts:{ts};`, compara em tempo constante e confere prazo, ambiguidades e igualdade corpo/query. Segredos de sandbox/produção são separados. Aceita timestamp em segundos ou milissegundos. Só retorna sucesso depois de persistir a entrega deduplicada; não guarda corpo completo nem dados de cartão.

O worker busca até 50 notificações a cada 30 segundos, com lease e escopo independente. Consulta o PaymentId na conta/conexão original e valida recebedor, ambiente, correlação, moeda e valor antes de alterar a venda. O estado recebido no body não decide o resultado.

Cada PaymentId possui lançamento próprio. Repetição não duplica captura, estoque ou cupom. Um segundo pagamento aprovado da mesma preferência gera captura separada e estorno do adicional, preservando a venda original. Recusa de uma tentativa não encerra toda a preferência. O método efetivamente pago é gravado no lançamento; o método solicitado permanece no snapshot.

Consultas periódicas, a cada cinco minutos, recuperam notificações perdidas, inclusive após cancelamentos. Cada ID encontrado na busca é consultado diretamente. Atualizações antigas não sobrescrevem o resultado aplicado. O worker apaga verificadores PKCE expirados.

Reembolso integral pelo painel cancela/devolve estoque e cupom quando o pedido ainda permite cancelamento. Após envio, registra o dinheiro devolvido sem inventar retorno físico nem mudar a etapa logística. Parcial, contestação e estados não suportados ficam em revisão. Pendências financeiras associadas ao comprador bloqueiam a exclusão da conta.

No provedor, configurar tópico `payment` e assinatura da aplicação que corresponde à credencial utilizada. O checkout envia a `notification_url` da conexão; ela pode prevalecer sobre a URL geral do painel. Segredo de assinatura é diferente de Client Secret. A configuração remota pelo MCP depende das URLs HTTPS reais.

## Validação e próximos passos

Passaram 84 verificações com PostgreSQL isolado e HTTP do Mercado Pago simulado, além das 143 verificações anteriores de checkout, estornos e exclusão de conta. Cobrem OAuth/PKCE/cookie/replay/expiração/revogação, identidade, tokens, preferência interrompida, assinatura, endpoints HTTP, concorrência, captura adicional, aprovação tardia, método real, saldo Mercado Pago, corpo vazio no reembolso integral, estorno/cancelamento interrompidos, falha anterior ao envio, pedido gratuito, consulta sem webhook, revisão e renovação.

A validação com usuários de teste do provedor confirmou o checkout, o webhook assinado, o pagamento no pedido e a movimentação de estoque em banco local separado, acessível por túnel HTTPS. O estorno financeiro permanece pendente de validação: o provedor recusou sua criação com HTTP 401 e causa 7, mesmo com as permissões gerais habilitadas. Há um [relato equivalente no SDK oficial](https://github.com/mercadopago/sdk-nodejs/issues/419), sem solução conclusiva publicada.

Em 18/11/2026, uma nova compra fictícia de BRL 20,00 foi aprovada e confirmada pela reconciliação automática, com uma única captura. O novo estorno integral, enviado pela API já corrigida para usar corpo vazio, voltou a receber `401 Unauthorized use of live credentials`, causa `7`. Uma chamada diagnóstica com a mesma chave persistida confirmou a resposta. Portanto, corrigir o corpo não resolveu a recusa de autorização. O provedor continuou retornando valor reembolsado zero; a operação permanece `OutcomeUnknown`, recuperada somente por consulta. O cancelamento comercial devolveu o estoque uma única vez, sem registrar reembolso fictício. O teste anterior e sua operação foram preservados. No novo teste, a confirmação ocorreu por consulta periódica; não havia webhook assinado associado até a verificação final.

Para o ambiente de implantação, faltam URLs definitivas, segredos no hospedeiro, aplicação das migrações, frontend consumindo esses contratos e conclusão da homologação do reembolso. Produção, operação de estorno parcial/contestação, revogação remota da conexão e Melhor Envio não são habilitados por esta entrega.
