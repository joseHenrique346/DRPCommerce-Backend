# Integrações externas — situação e próximos passos

Atualizado em 03/11/2026. O Store já possui catálogo por SKU, estoque, carrinho, checkout, cupons e ciclo do pedido. **Mercado Pago e Melhor Envio foram escolhidos**, e as contas já existem; os adapters ainda não foram implementados. Emissão fiscal depende de definição/contratação de API e habilitação do emitente.

O roteiro antigo de comparar gateways e serviços de envio deixa de ser requisito para escolher essas duas integrações. Este documento registra a preparação restante; não significa que houve homologação, transação real ou contratação fiscal.

## Estado do sistema

| Capacidade | Implementado | Pendente |
| --- | --- | --- |
| Venda Store | Checkout idempotente, totais do servidor, compromisso de estoque/cupom, numeração e histórico. | Ligar pagamento e frete reais preservando essas invariantes. |
| Pagamento | IPaymentGateway, sessão/instruções e aplicação interna de resultado; ManualPaymentGateway registrado. | Adapter Mercado Pago, conexão por loja, notificações validadas, consulta e reconciliação. |
| Reembolso | Permissão orders.refund no cancelamento pago, devolução de estoque/cupom e registro manual. | Dinheiro devolvido pelo provedor, estado pendente e recuperação; chamada externa fora da transação com lock. |
| Frete | Dimensões/peso por produto e sobrescritas por SKU; Shipment com dados de envio. | Cotar, selecionar, congelar custo, comprar/gerar/imprimir etiqueta e rastrear pelo Melhor Envio. ShippingCost permanece zero no cálculo atual. |
| Fiscal | Entidade Invoice e referências comerciais. | Emitente/perfil fiscal, transmissão, autorização, XML/DANFE, consulta e cancelamento. |
| Drop | Evento, inscrição, entrada na fila e consultas privadas. | Admissão, reserva no estoque canônico, correlação com venda Store e recuperação entre bancos. |

Fontes internas: [checkout/pedidos](../Store/Application/OrdersAndCheckout.md), [endpoints](../Store/Api/Endpoints.md), [estoque](../Store/Application/ProductsAndInventory.md), [Store/Drop](../Drop/Infrastructure/StoreIntegration.md).

## Decisões do recorte

- A loja cliente deve receber a venda na própria conta; configuração, credencial e IDs externos precisam distinguir loja e ambiente.
- A implementação inicial de pagamento deve priorizar o caminho completo de uma venda, confirmação e estorno integral. Não ampliar para cobrança de assinatura ou divisão financeira complexa do SaaS agora.
- O Store continua dono do estoque por SKU e de seu pedido financeiro. A venda originada no Drop precisa de correlação que impeça duas cobranças, duas notas ou duas baixas.
- Homologação de pagamento/frete/fiscal não representa dinheiro liquidado, postagem real ou documento fiscal válido em produção.
- CRM e ERP completos ficam fora do MVP; manter contratos internos claros não exige contratar esses sistemas agora.

## Mercado Pago — entrega mínima pendente

1. Definir aplicação, ambiente, conexão autorizada da conta de cada vendedor e guarda/renovação de credenciais no backend.
2. Implementar adapter atrás de IPaymentGateway, com tentativa persistida, referência do pedido e IDs externos separados conforme o produto/API escolhido.
3. Criar cobrança a partir do total canônico do Store; o comprador não escolhe recebedor, moeda ou preço final livremente.
4. Validar notificação e consultar o pagamento na API; retorno do navegador não confirma venda. Conferir conta, ambiente, valor, moeda e correlação antes de aplicar resultado.
5. Deduplicar callbacks e manter worker de reconciliação. Timeout pode ter resultado desconhecido; não repetir efeitos financeiros cegamente.
6. Tratar pagamento depois de expiração do pedido sem vender novamente o estoque já liberado.
7. Refatorar estorno para intenção persistida, chamada externa fora do lock e confirmação/reconciliação. O comportamento do adapter manual não basta para um provedor real.

Detalhes de endpoint, autenticação, requisitos de conta e disponibilidade devem ser confirmados na documentação oficial do produto escolhido durante a implementação. Não há webhook público Mercado Pago pronto apenas por existir ApplyPaymentResultCommand.

## Melhor Envio — entrega mínima pendente

1. Configurar ambiente, aplicação/conexão da loja, remetente e credenciais separadas.
2. Estruturar endereço e contato/documentação exigidos; padronizar unidades de peso/dimensões e embalagem do recorte.
3. Cotar carrinho/endereço no backend e persistir opções, validade, serviço e valor. Checkout seleciona uma cotação válida, não um preço arbitrário do navegador.
4. Congelar o frete no pedido e incluí-lo uma vez no total enviado ao pagamento.
5. Após pagamento e documento fiscal quando exigido, permitir ao operador contratar/gerar etiqueta, consultar pendências e imprimir com acesso restrito.
6. Persistir IDs externos para recuperar timeout sem comprar duas etiquetas; separar etiqueta comprada, gerada, postada e entregue.

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
