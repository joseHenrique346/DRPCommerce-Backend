# Diagnóstico de estorno no sandbox do Mercado Pago

Investigação de 18/11/2026. A análise documental foi seguida por uma única comparação do estorno já autorizado, usando JSON `{}` e a mesma chave da operação persistida. Não foram criados pagamentos, tokens ou autorizações, nem alteradas permissões. O provedor recusou novamente a criação do reembolso. As etapas anteriores permanecem registradas separadamente.

## Conclusão

Não foi encontrada uma autenticação adicional obrigatória imediatamente antes do reembolso de um pagamento aprovado no Checkout Pro via Preferences. O contrato público é `POST /v1/payments/{payment_id}/refunds`, usando o Access Token do vendedor e `X-Idempotency-Key`. O token direto do painel é previsto na referência do endpoint. OAuth é necessário para obter autorização de vendedores terceiros; não é uma etapa extra a repetir para cada reembolso.

O token configurado foi comparado por SHA-256, sem expor seu valor, ao token da seção **Test (Sandbox)** retornado pelo MCP para a aplicação principal. São iguais. O próprio MCP identifica sua origem como vendedor automático `3857410022`, aplicação `3054928538483294`. Portanto, usar esse token não é, por si só, uma troca equivocada pelo token da conta real proprietária do projeto.

A hipótese principal continua sendo autorização efetiva da credencial para criar reembolsos, ou restrição do ambiente de contas de teste. Ela ainda não é uma causa definitivamente comprovada. A comparação com JSON `{}`, `Content-Type: application/json` e `Accept: application/json` também retornou HTTP 401, causa `7`; não resolveu a recusa. Permanece a confirmação independente do saldo disponível. O retorno numérico `7` não possui explicação específica publicada na referência de reembolsos consultada.

## Qual API está integrada

Este projeto cria preferências em `/checkout/preferences` e consulta pagamentos numéricos em `/v1/payments`. Portanto, aplica-se **Checkout Pro via Preferences API**.

Checkout Pro via Orders tem outro contrato: `/v1/orders/{order_id}/refund`, com identificadores `ORD...`. O `merchant_order_id` numérico retornado pelo nosso checkout não deve ser usado no lugar de um ID dessa Orders API. Não há necessidade demonstrada de migrar para Orders para estornar nossos pagamentos.

Fontes: [reembolsos de Preferences](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-pro-preferences/create-refund/post), [reembolsos de Orders](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-pro-orders/refund-order/post).

## Contas de teste: funções e limites

- **Vendedor:** representa o recebedor, com credenciais para configurar a cobrança e consultar seus recursos. No Checkout Pro, a aplicação pode receber automaticamente um vendedor e credenciais de teste com prefixo `APP_USR`.
- **Comprador:** representa quem faz a compra, podendo usar cartões oficiais de teste ou saldo fictício. Comprador e vendedor devem ser diferentes e do mesmo país.
- **Integrador:** representa a plataforma em testes de marketplace/OAuth. Não substitui o token do recebedor na operação de um pagamento específico.
- Os cartões de teste e os dados do titular simulam aprovação, recusa e pendência. Na documentação de Preferences consultada, os meios offline, como Pix e boleto, validam a geração da cobrança pendente; isso não equivale a demonstrar liquidação completa.
- É possível manter até 15 contas de teste simultaneamente; a documentação informa que ainda não podem ser excluídas. O saldo fictício pode ser alterado pelo painel.
- Existem limitações administrativas: algumas páginas de credenciais e qualidade não ficam acessíveis ao entrar como test user. Os relatórios de Liberações de contas de teste podem ser gerados sem dados. Portanto, a descrição geral de recursos semelhantes aos reais não significa paridade de todos os serviços.
- O prefixo `APP_USR` e o indicador `live_mode` isoladamente não decidem se foi usada uma conta real. No nosso caso, a identidade remota e a origem automática de teste foram conferidas.

Fontes: [contas de teste de Preferences](https://www.mercadopago.com.br/developers/pt/docs/checkout-pro-preferences/test-accounts), [compras de teste de Preferences](https://www.mercadopago.com.br/developers/pt/docs/checkout-pro-preferences/integration-test/test-purchases), [credenciais automáticas, anúncio de 19/11/2025](https://www.mercadopago.com.br/developers/pt/news/2025/11/19/Streamlined-integration-testing-with-automatic-credentials), [limitações dos relatórios de Liberações](https://www.mercadopago.com.br/developers/pt/docs/checkout-api-payments/additional-content/reports/released-money/introduction).

## Sequência de autenticação e reembolso

1. Obter o Access Token adequado: diretamente do painel para os recursos próprios ou por autorização OAuth do vendedor terceiro.
2. Criar a preferência com a conta do vendedor de teste e concluir a compra como comprador fictício.
3. Consultar o `payment_id` real com o token do recebedor. Conferir conta, valor, moeda, captura e reembolsos já existentes.
4. Para um pagamento já aprovado/capturado, criar o reembolso diretamente; não cancelar o pagamento antes nem capturá-lo de novo.
5. Consultar o pagamento e seus reembolsos para confirmar o resultado financeiro. O webhook pode comunicar a mudança, mas seu recebimento não é uma autorização necessária para chamar o endpoint.

A documentação de Preferences estabelece reembolso em até 180 dias e saldo suficiente do vendedor. No cartão, o retorno ocorre pela fatura; outros meios podem devolver à conta do pagador. Se uma compra envolver dois pagamentos, cada `payment_id` precisa ser tratado individualmente.

No fluxo OAuth, `/oauth/token` troca um código de autorização por token; o campo `test_token=true` seleciona a emissão de credenciais de sandbox. Nosso fluxo OAuth já inclui esse campo em sandbox. O teste financeiro em questão usou a conexão estática com o token do painel e não executou essa troca. Esse campo no body de OAuth não é o mesmo que o header `X-Test-Token` de uma chamada de recurso.

Fontes: [OAuth e obtenção de tokens](https://www.mercadopago.com.br/developers/pt/docs/security/oauth/creation), [reembolsos e cancelamentos de Preferences](https://www.mercadopago.com.br/developers/pt/docs/checkout-pro-preferences/additional-settings/refunds-and-cancellations).

## Auditoria da requisição

| Item | Contrato / observação | Estado observado |
| --- | --- | --- |
| Método e URL | `POST https://api.mercadopago.com/v1/payments/{id}/refunds` | Corretos. |
| Identificador | `payment_id`, não preferência, pedido local ou merchant order | Pagamento `189303189317` conferido. |
| Authorization | `Bearer` com o Access Token do recebedor | Token de teste do MCP igual ao configurado; `/users/me` confirma o vendedor. |
| X-Idempotency-Key | Uma chave persistida por intenção de estorno | Presente; diagnóstico reutilizou a mesma chave da operação 5. |
| Valor no integral | Omitir `amount` | A chamada corrigida não enviou `amount`. |
| Content-Type e JSON | SDK oficial envia JSON `{}` e `application/json` | Adapter atual envia `Content=null`, sem Content-Type explícito. Diagnóstico REST sem body e comparação posterior com JSON `{}` explícito receberam HTTP 401, causa `7`. |
| Captura | Pagamento precisa estar capturado para reembolso | `captured=true`, `status=approved`, `status_detail=accredited`. |
| Liberação | Não confundir aprovação com saldo disponível | Pagamento retorna `money_release_status=released`; saldo total disponível da conta ainda não confirmado. |
| Resultado | Só confirmar estorno após evidência financeira | `transaction_amount_refunded=0`, nenhum reembolso; estado local não foi marcado como sucesso. |

Uma precisão sobre "body vazio": o SDK Node oficial implementa o estorno integral com `JSON.stringify({})`. O SDK .NET cria `PaymentRefundCreateRequest` com `Amount=null`, ignora propriedades nulas na serialização e envia conteúdo JSON. A exigência inequívoca é **omitir `amount`**. Não é correto afirmar que o único formato aceito é necessariamente um corpo HTTP de zero bytes. A comparação diagnóstica posterior reproduziu `{}` com `Content-Type: application/json` e `Accept: application/json`, preservando a mesma intenção e sua chave; recebeu a mesma recusa de autorização.

Fontes: [SDK Node: estorno integral](https://github.com/mercadopago/sdk-nodejs/blob/master/src/clients/paymentRefund/total/index.ts), [SDK Node: HTTP e headers](https://github.com/mercadopago/sdk-nodejs/blob/master/src/utils/restClient/index.ts), [SDK .NET: PaymentRefundClient](https://github.com/mercadopago/sdk-dotnet/blob/master/src/MercadoPago/Client/Payment/PaymentRefundClient.cs), [SDK .NET: serializer](https://github.com/mercadopago/sdk-dotnet/blob/master/src/MercadoPago/Serialization/DefaultSerializer.cs), [SDK .NET: transporte](https://github.com/mercadopago/sdk-dotnet/blob/master/src/MercadoPago/Http/DefaultHttpClient.cs).

## Headers especiais investigados

- **X-Test-Token:** existe como opção genérica `testToken` no SDK Node e só é enviado quando solicitado. A documentação de Split 1:N via Orders o exige em seu fluxo de testes; não foi encontrada a mesma exigência na referência de reembolso via Payments/Preferences. O exemplo e o teste de estorno integral do SDK Node não o configuram. Portanto, é uma hipótese para uma comparação controlada, não um requisito demonstrado ausente no nosso endpoint. Um GET do nosso pagamento com esse header retornou o mesmo pagamento aprovado e `live_mode=true`; isso não prova como o POST se comportaria.
- **X-Test-Scope: sandbox:** documentado para a antiga Payment Intent API de Point; não é requisito identificado para o nosso Checkout Pro via Preferences.
- **X-Render-In-Process-Refunds:** permite representar reembolsos Pix em processamento. Não resolve autorização de um estorno de cartão.
- **X-Meli-Session-Id:** está associado ao dispositivo e ao fluxo de pagamento; não aparece como requisito de autenticação do reembolso na referência consultada.
- **X-Integrator-Id, X-Platform-Id e X-Corporation-Id:** opções condicionais do SDK; não devem ser inventadas nem usadas como substitutas da autorização do vendedor.
- **x-signature / x-request-id de webhook:** validam a notificação recebida pela nossa API. Não são assinaturas que precisamos criar para autorizar o POST de reembolso. O `x-request-id` da resposta do provedor serve para rastrear o diagnóstico.

Fontes: [tipos/opções do SDK Node](https://github.com/mercadopago/sdk-nodejs/blob/master/src/types.ts), [teste de estorno integral do SDK](https://github.com/mercadopago/sdk-nodejs/blob/master/e2e/paymentRefund/total.spec.ts), [Split 1:N](https://www.mercadopago.com.br/developers/en/docs/checkout-api-orders/resources/split-payments-1-n), [migração de Point](https://www.mercadopago.com.br/developers/pt/docs/mp-point/resources/migrate-payment-intent-to-orders), [reembolso Pix em processamento](https://www.mercadopago.com.br/developers/pt/docs/checkout-api-payments/payment-management/cancellations-and-refunds/refund-pix).

## Permissões: o que a pesquisa sustenta

A documentação de criação de pagamentos associa a mensagem `Unauthorized use of live credentials` à ausência do scope `payment`. A referência de **reembolso**, porém, descreve HTTP 401 genericamente como token inválido e não mapeia a causa `7` a uma configuração específica. Não se pode transportar automaticamente a explicação de um endpoint para outro.

A aplicação do vendedor possui `read`, `write`, `offline_access` e escopos específicos, incluindo `urn:mp:online:order:payment/read-write`. Isso não é prova de que o token tenha autorização efetiva de escrita para `/v1/payments/{id}/refunds`. A lista da aplicação não é uma introspecção completa do token. Também não foi comprovado que nosso token tenha `urn:mp:online:payments:refunds/read-only`.

Na [issue 419 do SDK oficial](https://github.com/mercadopago/sdk-nodejs/issues/419#issuecomment-5636973687), um colaborador, em 11/09/2026, mantém duas hipóteses: restrição de TESTUSER e escopos OAuth de reembolso somente leitura. Não há solução definitiva publicada nessa discussão. O reembolso pelo painel foi relatado por participantes, mas ainda não foi executado na nossa conta.

Há uma discussão antiga que recomenda evitar tokens temporários de `client_credentials` para essas operações. Ela não resolve o nosso caso: usamos diretamente o token do painel do vendedor automático, agora também conferido com o MCP. Não há indicação de que chamar `client_credentials` de novo concederia a permissão faltante.

Fontes: [erro descritivo em criação de pagamentos](https://www.mercadopago.com.br/developers/en/reference/online-payments/subscriptions/create-payment/post), [referência de reembolso](https://www.mercadopago.com.br/developers/pt/reference/online-payments/checkout-pro-preferences/create-refund/post), [discussão histórica](https://github.com/mercadopago/DEPRECATED-sdk-php/issues/133).

## Evidências remotas desta etapa

- Vendedor `3857410022`: tag `test_user`, país MLB, status ativo, compras e vendas permitidas, termos do Mercado Pago aceitos. Não foi observada ação obrigatória pendente no status da conta.
- Pagamento `189303189317`: recebedor `3857410022`, comprador `3853433813`, BRL 20,00, `approved/accredited`, `captured=true`, `money_release_status=released`, valor reembolsado zero.
- O endpoint consultado de saldo retornou `403 ForbiddenApiError`. Não foi presumido saldo zero nem falta de saldo; é necessário conferir o disponível pelo painel do vendedor fictício ou por um recurso autorizado para essa conta.
- GET do pagamento com `X-Test-Token: true`: HTTP 200, mesmo ID e estado. Esse header não foi adicionado ao POST de comparação.
- O teste financeiro anterior com corpo ausente retornou HTTP 401/causa 7; request ID `f15d3a0d-27ba-48d5-a6fb-7b39914b22c6`, em `2026-11-18T20:33:50.4890868Z`.
- Comparação com JSON `{}` em 18/11/2026 às 19:29:42 de São Paulo: dois bytes UTF-8, Content-Type e Accept JSON, sem redirecionamentos, mesmo token e chave da operação 5. Resposta HTTP 401, `Unauthorized use of live credentials`, causa `7`; request ID `86515a20-2591-4ee6-9b40-06686be74d1f`, timestamp UTC `2026-11-18T22:29:42.4923398Z`.
- GETs posteriores do pagamento e da lista de reembolsos confirmaram `approved`, valor reembolsado zero e lista vazia. Não foi criada outra intenção local de estorno ou chave de idempotência.

## Comparação concluída e próximo diagnóstico

A comparação do formato HTTP já foi realizada no pagamento fictício existente e não resolveu a recusa. A sugestão da IA de usar uma nova chave não foi aplicada: trata-se da mesma intenção de estorno, cujo histórico incerto continua preservado. Uma nova chave não deve ser criada apenas para contornar a recusa da operação existente. Não há necessidade demonstrada de outra compra para continuar o diagnóstico.

O diagnóstico a pedir ao Mercado Pago é específico: quais privilégios efetivos essa credencial de vendedor automático tem para **criar** refunds via Payments/Preferences e se essa combinação de aplicação/test user é suportada. Anexar request ID, horário, IDs e formato sanitizado da requisição. Confirmar também o saldo disponível. Sucesso pelo painel isolaria melhor a diferença entre a autorização da sessão web e a autorização da API, mas não homologaria sozinho o POST da nossa aplicação.
