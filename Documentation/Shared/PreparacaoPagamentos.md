# Conexões e tentativas de pagamento

Implementado na tarefa #93 em 16/11/2026. Esta entrega prepara os três primeiros itens da integração: conexão por loja/ambiente, intenção persistida antes da chamada externa e identidades externas separadas. O adapter ativo continua sendo `ManualPaymentGateway`; esta entrega não autoriza contas no Mercado Pago/Melhor Envio nem faz chamadas a essas APIs.

## Conexões de provedores

`ProviderConnection` guarda loja, provedor (`mercado-pago` ou `melhor-envio`), ambiente (`Sandbox = 1`, `Production = 2`), conta externa, escopos, validades, operador da autorização e estado da conexão. Há uma conexão por loja/provedor/ambiente. Reautorização preserva a conta externa: trocar de vendedor exige uma evolução que preserve as conexões históricas, não editar a conta de pagamentos anteriores.

`ProviderConnectionService.SaveAuthorizedAsync` é o ponto interno para o futuro callback OAuth, **depois** da validação de state/PKCE, identidade da loja/operador e identidade da conta retornada pelo provedor. Não há endpoint para cadastrar tokens arbitrários ou declarar uma conta conectada pelo navegador. As chamadas OAuth, cadastro de aplicações, state/PKCE e revogação remota pertencem à próxima etapa dos adapters.

`GetTokensAsync` resolve a conexão no escopo exato de loja/provedor/ambiente. Quando a validade está próxima do fim, usa `IProviderTokenRefresher`, com uma lease persistida e timeout. A atualização compara versão/lease/estado conectado: desconectar ou reautorizar durante a chamada impede que uma resposta antiga sobrescreva a autorização atual. Erro de refresh mantém a lease até expirar; rotação remota inconclusiva pode exigir reautorização. O adapter deve preservar o refresh token anterior quando a resposta não retornar outro.

Os tokens são criptografados com ASP.NET Core Data Protection, com finalidade vinculada também à loja, provedor, ambiente e conta. Copiar o conteúdo criptografado para outro escopo não permite descriptografá-lo. Tokens e conteúdo criptografado não aparecem nos DTOs HTTP. Objetos de credenciais têm serialização e representação textual redigidas.

Configuração antes de salvar a primeira autorização externa:

```text
ProviderIntegrations__KeyRingPath=<diretório privado e persistente fora do repositório e wwwroot>
ProviderIntegrations__KeyEncryptionCertificateThumbprint=<certificado, quando usado>
```

Sem `KeyRingPath`, a gravação de novas credenciais é recusada; o modo manual continua funcionando. Preserve o diretório e suas chaves em reinícios, backups e entre réplicas que precisam ler os mesmos tokens. Windows usa DPAPI quando não há certificado configurado. Para réplicas/mudança de host, configure um mecanismo de proteção de chaves compartilhável, como o certificado indicado. Em outros sistemas, configure certificado e acesso restrito ao diretório conforme o ambiente. Perder as chaves impede recuperar os tokens. [Configuração oficial do Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview).

Endpoints de backoffice, ambos com `settings.manage`, loja obtida da identidade autenticada e sem tokens nas respostas:

| Método | Rota | Resultado |
| --- | --- | --- |
| GET | `/api/provider-connections` | Metadados das conexões da loja. |
| DELETE | `/api/provider-connections/{provider}/{environment}` | Desconexão local idempotente, remoção dos tokens e invalidação de refresh em andamento. Mantém IDs e histórico. |

Exemplo: `DELETE /api/provider-connections/mercado-pago/1`. Desconexão local não afirma que houve revogação no provedor nem cancela pagamentos já iniciados.

## Intenção e processamento

O checkout grava `PaymentAttempt` na **mesma transação** de pedido, pagamento, reserva e cupom, antes de qualquer criação remota. A tentativa congela recebedor/conexão/ambiente, valor, moeda, cliente, método, prazo, chave de idempotência e `ExternalReference` opaca. Alterar as configurações da loja depois não muda esse pedido de pagamento.

Depois do commit, `PaymentAttemptService` obtém uma lease por atualização condicional no PostgreSQL. Somente o primeiro processo pode executar a criação. A chamada ao gateway ocorre fora da transação do checkout e dos locks de estoque.

| Estado | Próxima ação permitida |
| --- | --- |
| Requested | Primeira criação, com pedido/pagamento pendentes, prazo válido e conexão ativa quando externa. |
| Processing com lease ativa | Outra execução aguarda; não cria novamente. |
| Processing abandonado | Consultar o provedor pela mesma correlação. |
| OutcomeUnknown | Consultar o provedor; ausência de resultado mantém a pendência. |
| Created | Reutilizar os dados persistidos. |

O worker consulta até 50 intenções a cada 30 segundos. Falhas ficam pendentes com intervalo mínimo de um minuto; lease vencida não autoriza outro POST. `RecoverPaymentAsync` deve somente consultar: `null` mantém o resultado desconhecido. Recuperação pode ocorrer mesmo após cancelamento/expiração para preservar as referências de uma chamada já iniciada. Ela não marca o pedido pago, não reativa estoque e não estorna dinheiro. Esses efeitos continuam nos fluxos comerciais e na futura confirmação validada.

Um processo que perde sua lease não pode concluir ou sobrescrever o trabalho assumido por outro. Resposta externa salva e instruções em `Transaction` são persistidas juntas. Se o banco falhar depois da resposta, o próximo processamento consulta a tentativa.

Repetir checkout não cria outro pedido/tentativa. Pedidos manuais antigos, ainda pendentes e sem referência, podem receber uma intenção de compatibilidade. Essa reconstrução **não é feita para provedores reais**, pois referência local ausente não comprova ausência de cobrança externa.

## Identificadores e contratos do adapter

- `PaymentAttempt.ExternalReference` e `IdempotencyKey`: valores estáveis gerados pelo servidor e usados em criação/consulta.
- `PreferenceId`: identidade do checkout do provedor, separada da referência legada em `Transaction`.
- `PaymentExternalPayment.PaymentId`: uma preferência/tentativa pode ter vários pagamentos. Índice único por provedor/ambiente/conta/PaymentId impede apropriar a mesma cobrança em outro pedido, inclusive entre lojas que compartilhem uma conta.
- Chaves estrangeiras compostas mantêm conexão, tentativa e pagamentos no mesmo escopo de loja/provedor/ambiente/conta.
- `RegisterVerifiedPaymentAsync`: entrada interna para associar pagamentos **após consulta autenticada ao provedor**. Confere escopo, correlação, valor e moeda; repetição é idempotente. Não confirma pagamento por si só e não é um webhook público.

`IPaymentGateway.GetContextAsync` deve selecionar configuração/conexão da loja, sem criar operações remotas. `CreatePaymentAsync` recebe o snapshot persistido, contexto e correlação. `RecoverPaymentAsync` consulta exatamente esse mesmo escopo; nunca muda recebedor ou cria outra cobrança. A chave local de idempotência não garante que todo endpoint externo aceite o mesmo header: o adapter deve respeitar o contrato específico da API.

`PaymentSession` diferencia `Reference` legada, `PreferenceId` e `PaymentId` opcional. Se a resposta da criação já tiver um PaymentId conhecido, ele é associado sem duplicar um ID previamente recebido pelo fluxo de consulta. Retorne apenas URLs HTTPS do checkout e instruções públicas ao comprador, nunca credenciais ou payload bruto do SDK.

`PaymentInfoResponse` mantém os campos existentes e acrescenta `attempt`: `id`, `creationStatus`, `currencyCode`, `preferenceId` e `paymentIds`. O frontend deve admitir pagamento sem URL enquanto a criação está em processamento ou com resultado desconhecido e consultar o detalhe do pedido. O enum `creationStatus` descreve criação da sessão; não substitui aprovação/recusa financeira.

## Banco e validação

Migração `AddProviderConnectionsAndPaymentAttempts`: cria `conexao_provedor`, `tentativa_pagamento` e `pagamento_externo`, índices e FKs. A implantação exige aplicar a migração antes de executar esta versão da API/worker. A migração não contém tokens nem cria conexões fictícias.

```powershell
dotnet ef database update --project DROP-Ecommerce/StoreCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Store/StoreCommerce.Api --context StoreCommerceDbContext
```

Validação local com 42 verificações em PostgreSQL 17 temporário e isolado: migrações, criptografia e isolamento, renovação concorrente/desconexão, processamento concorrente, resposta perdida, lease abandonada, deduplicação de IDs, valor/moeda/recebedor e checkout real com reserva única. Não houve transação nos provedores nem atualização dos bancos existentes.

## Próximas etapas da #93

Implementar OAuth e adapters reais; webhook assinado e consulta autoritativa do estado financeiro; estorno fora da transação de catálogo; cotação de frete no checkout e etiquetas. Antes de habilitar pagamentos reais, também adaptar cancelamento/estorno/consulta para usar os identificadores e a conexão congelados, preservar aprovação tardia e impedir confirmação manual indevida. Esta fundação não torna o fluxo manual apropriado para dinheiro real.
