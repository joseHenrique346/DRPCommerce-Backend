# Integrações externas — situação e próximos passos

Situação de 21/11/2026: as sete partes comerciais incluem [assinatura mensal da plataforma](PlanosEContratos.md)
e [catálogo, fila, reserva e pagamento DROP](../Drop/Application/FluxoComercial.md).
O [roteiro do TCC](ValidacaoComercialTcc.md) valida as APIs com provedores simulados;
isso não conclui homologação externa. Melhor Envio é a próxima integração, após essas partes.
As atualizações datadas abaixo preservam o histórico do planejamento.

Atualização de 23/11/2026. O Store implementa o OAuth do Melhor Envio, a
cotação dos itens selecionados e o registro do frete no pedido antes da
tentativa de pagamento. Os próximos marcos são validar uma conta no Sandbox
e implementar o ciclo de etiquetas. [Melhor Envio no Store](MelhorEnvio.md)

Atualização de 18/11/2026: [backend Mercado Pago](MercadoPago.md) implementado com OAuth, Checkout Pro, webhook assinado e reconciliação. Desabilitado por padrão; faltam URLs/segredos, frontend e homologação real em sandbox. Os registros anteriores abaixo devem ser lidos como histórico.

Atualização de 16/11/2026 na #93: implementada a [fundação de conexões e tentativas de pagamento](PreparacaoPagamentos.md), com tokens protegidos, snapshot financeiro, IDs separados e recuperação de criação interrompida. O adapter ativo continua manual; OAuth, webhooks e adapters reais permanecem pendentes. As tabelas abaixo descrevem a base anterior e devem ser lidas junto dessa atualização.

Atualizado em 03/11/2026. O Store já possui catálogo por SKU, estoque, carrinho, checkout, cupons e ciclo do pedido. **Mercado Pago e Melhor Envio foram escolhidos**, e as contas já existem; os adapters ainda não foram implementados. Emissão fiscal depende de definição/contratação de API e habilitação do emitente.

O roteiro antigo de comparar gateways e serviços de envio deixa de ser requisito para escolher essas duas integrações. Este documento registra a preparação restante; não significa que houve homologação, transação real ou contratação fiscal.

## Estado do sistema

| Capacidade | Implementado | Pendente |
| --- | --- | --- |
| Venda Store | Checkout idempotente, totais do servidor, compromisso de estoque/cupom, numeração e histórico. | Ligar pagamento e frete reais preservando essas invariantes. |
| Pagamento | Seleção por loja, adapter Mercado Pago, OAuth, webhook assinado e consulta/reconciliação; modo manual preservado. | Configuração do ambiente, frontend e homologação em sandbox. |
| Contratação SaaS | Assinatura mensal, upgrade, downgrade, sete dias de carência, concessão confirmada e reconciliação. | Configuração da conta da plataforma, frontend e homologação externa da recorrência. |
| Reembolso | Intenção persistida, I/O fora do lock, adapter de estorno integral e recuperação por consulta. | Homologação no provedor e fluxo operacional de parcial/contestação. |
| Frete | Dimensões/peso por produto e sobrescritas por SKU; Shipment com dados de envio. | Cotar, selecionar, congelar custo, comprar/gerar/imprimir etiqueta e rastrear pelo Melhor Envio. ShippingCost permanece zero no cálculo atual. |
| Fiscal | Entidade Invoice e referências comerciais. | Emitente/perfil fiscal, transmissão, autorização, XML/DANFE, consulta e cancelamento. |
| Drop | Catálogo próprio, admissão, CPF/evento, reserva por variação, estoque exclusivo, pedido próprio e ponte financeira Store com recuperação. | Frontend, homologação externa, operação de revisão financeira e capacidade sob carga. |

Fontes internas: [checkout/pedidos](../Store/Application/OrdersAndCheckout.md), [endpoints](../Store/Api/Endpoints.md), [estoque](../Store/Application/ProductsAndInventory.md), [Store/Drop](../Drop/Infrastructure/StoreIntegration.md).

## Decisões do recorte

- A loja cliente deve receber a venda na própria conta; configuração, credencial e IDs externos precisam distinguir loja e ambiente.
- A assinatura SaaS foi aprovada e implementada em fluxo separado das vendas dos lojistas. A investigação de estorno foi adiada; divisão financeira complexa continua fora do MVP.
- Estoque fica sob controle exclusivo de um módulo. Após transferência comprovada, o DROP controla seu saldo/pedido e usa uma intenção financeira separada no Store; não cria outro pedido Store ou outra baixa.
- Homologação de pagamento/frete/fiscal não representa dinheiro liquidado, postagem real ou documento fiscal válido em produção.
- CRM e ERP completos ficam fora do MVP; manter contratos internos claros não exige contratar esses sistemas agora.

## Mercado Pago — base implementada e homologação pendente

Os itens de infraestrutura abaixo já possuem implementação descrita em [Mercado Pago](MercadoPago.md)
e [planos/assinaturas](PlanosEContratos.md). Conferir configuração e homologação conforme o
ambiente; não executar novamente o planejamento como se não houvesse adapter ou webhook.

1. Definir aplicação, ambiente, conexão autorizada da conta de cada vendedor e guarda/renovação de credenciais no backend.
2. Implementar adapter atrás de IPaymentGateway, com tentativa persistida, referência do pedido e IDs externos separados conforme o produto/API escolhido.
3. Criar cobrança a partir do total canônico do Store; o comprador não escolhe recebedor, moeda ou preço final livremente.
4. Validar notificação e consultar o pagamento na API; retorno do navegador não confirma venda. Conferir conta, ambiente, valor, moeda e correlação antes de aplicar resultado.
5. Deduplicar callbacks e manter worker de reconciliação. Timeout pode ter resultado desconhecido; não repetir efeitos financeiros cegamente.
6. Tratar pagamento depois de expiração do pedido sem vender novamente o estoque já liberado.
7. A base de [cancelamento/estorno persistido e recuperável](CancelamentoEstorno.md) está implementada. Falta ligar e homologar execução/consulta no adapter real.

Detalhes de endpoint, autenticação, requisitos de conta e disponibilidade devem ser confirmados na documentação oficial do produto escolhido durante a implementação. Não há webhook público Mercado Pago pronto apenas por existir ApplyPaymentResultCommand.

## Melhor Envio — entrega mínima pendente

1. Validar a autorização e a cotação no Sandbox da API publicada.
2. Estruturar o endereço, o contato e os documentos exigidos para a contratação do envio.
3. Após pagamento e documento fiscal quando exigido, permitir ao operador contratar e gerar a etiqueta, consultar pendências e imprimir com acesso restrito.
4. Persistir IDs externos para recuperar timeouts e evitar compras duplicadas. Separar etiqueta comprada, gerada, postada e entregue.

Receber a venda no Mercado Pago não compra o frete automaticamente. O custo/saldo do envio, regras da transportadora e disponibilidade do serviço precisam fazer parte da operação da loja. Cancelar pedido, etiqueta e pagamento são efeitos distintos.

## Emissão fiscal

Escolher API de NF-e para a operação piloto e confirmar custo, requisitos do emitente, certificado, UF e ambiente de homologação. A pesquisa anterior sugeriu Focus NFe como candidata; ela ainda não é um adapter implementado nem uma contratação confirmada no código.

Acrescentar configuração fiscal por loja, perfil fiscal validado dos produtos, dados do destinatário e snapshot coerente com preço/desconto/frete do pedido. Emissão precisa de referência estável, processamento/consulta, tratamento de rejeição, XML autorizado e DANFE protegido. Evitar um motor tributário genérico neste MVP; o perfil da operação deve ser validado pelo responsável fiscal.

Reembolso financeiro não cancela nota automaticamente. Cancelamento fiscal e eventual devolução de mercadoria exigem fluxo e estado próprios, conforme o caso. Não apresentar Invoice cadastrado como prova de autorização.

## Integrações complementares

| Necessidade | Recorte |
| --- | --- |
| E-mail transacional | Entrega segura para confirmação/recuperação de conta e mensagens de pedido. Hoje as ações de recuperação/confirmação estão indisponíveis. |
| Documentos privados | Armazenar XML/PDF/etiquetas com acesso por loja/cliente, backup e retenção definida. |
| Monitoramento | Detectar worker parado, callback atrasado, operação desconhecida, credencial expirada e falha fiscal/logística. |
| Cache/limites compartilhados do Drop | Planejar junto da meta de carga; não usar cache como fonte autoritativa de estoque. Ainda não está provisionado no projeto. |
| CEP | Conveniência opcional de preenchimento; permitir correção manual e validar dados logísticos/fiscais. |

## Ordem de execução e aceite

| Etapa | Resultado verificável |
| --- | --- |
| Dados e conexões | Loja/endereço/SKU corretos, credenciais protegidas e ambiente separado. |
| Pagamento e reconciliação | Uma operação cria uma venda; confirmação confere recebedor/valor e não baixa estoque duas vezes. Falha é recuperável. |
| Frete e checkout | Cotação selecionada e custo incluído no total; indisponibilidade não vira frete grátis. |
| Fiscal e etiqueta | Documento homologado quando houver emitente habilitado; etiqueta ligada ao pedido correto e acesso privado. |
| Operação | Estorno, expiração, repetição, indisponibilidade e pendências ficam explícitos ao operador. |
| Drop | Reserva por SKU convertida em uma venda, sem duplicar compromissos entre bancos. |

O cronograma de entrega até 30/11/2026 depende de concluir conexão de contas e habilitação fiscal cedo. Capacidade de milhares de compradores precisa de medição em ambiente representativo, além da implementação funcional. Esta atualização é documental: não cria projetos de testes, executa homologação ou altera contas externas.
