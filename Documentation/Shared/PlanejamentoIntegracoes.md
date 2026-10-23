# Planejamento de pesquisa: pagamentos, envios, CRM e ERP

## Tarefa para o integrante do TCC

**Título:** Levantar requisitos e propor a evolução do DRPCommerce para integrações externas.

**Objetivo:** pesquisar como integrar pagamentos e envios ao fluxo de venda e como preparar o sistema para conectar, futuramente e por opção de cada loja, o CRM e o ERP utilizados pelo cliente. A entrega deve permitir que a equipe decida o que implementar primeiro, quais dados faltam e quais mudanças serão necessárias no backend.

**Tipo de atividade:** pesquisa e planejamento técnico. A implementação dos conectores será uma tarefa posterior.

**Prioridade sugerida:** pagamentos e envios primeiro; preparação para CRM e ERP em seguida. A equipe deve confirmar esse recorte conforme a entrega do TCC.

**Responsável e prazo:** a definir pela equipe, considerando a disponibilidade do integrante e o cronograma do TCC.

Este roteiro foi elaborado com leitura local do projeto em 21/10/2026, sem pesquisa externa ou escolha de fornecedores. Os itens abaixo são perguntas e hipóteses a investigar, não conclusões sobre serviços disponíveis.

## 1. Ponto de partida do projeto

| Base identificada | Como isso orienta a pesquisa |
| --- | --- |
| `Enterprise` representa a loja e o tenant; `Customer` pertence a uma loja. | Investigar configuração, credenciais, permissões e identificação de registros externos por loja. |
| Store é a fonte canônica de lojas, clientes e produtos. Drop possui banco próprio e consulta o Store por integração interna. | Definir onde cada operação externa fica e como os dois módulos compartilham seu resultado. |
| `Order` e `DropOrder` possuem valores, endereço, frete e status de pedido/pagamento. | Mapear o ciclo de compra completo e as transições de estado necessárias. |
| `Transaction` e `DropTransaction` possuem campos de referência, provedor e payload do gateway. | Verificar se o modelo comporta tentativas, confirmação assíncrona, taxas, cancelamentos e reembolsos. |
| `Shipment` possui transportadora, rastreamento, custo, endereço e datas. No Store, um pedido possui uma coleção de envios. | Identificar dados adicionais para cotar, contratar, emitir etiqueta e acompanhar a entrega. |
| `Product` possui SKU, código de barras, peso e dimensões; `OrderItem` preserva nome, SKU, quantidade e valores. | Verificar unidades, embalagem, identificação externa e dados históricos necessários para as integrações. |
| `Invoice` registra dados como número, série, chave e arquivo. | Investigar quem emite a nota e como o resultado chega ao sistema e ao fluxo de envio. |

Essas estruturas são um ponto de partida; sua existência não comprova que os fluxos de checkout, cobrança, emissão fiscal ou contratação de frete estejam completos. O levantamento deve separar modelo existente, operação implementada e requisito ainda pendente.

Leituras locais iniciais:

- [Identidade, lojas e isolamento](../Store/Domain/Identity.md).
- [Integração Store/Drop](../Drop/Infrastructure/StoreIntegration.md).
- [Organização da infraestrutura](Infrastructure.md), confrontando os detalhes com o código atual.
- Entidades em `DROP-Ecommerce/StoreCommerce.Domain/Entity/`: `Order`, `OrderItem`, `Transaction`, `Shipment`, `Product`, `Customer` e `Invoice`.
- Entidades em `DROP-Ecommerce/DropCommerce.Domain/Entity/`: `DropOrder`, `DropOrderItem`, `DropTransaction` e `DropReservation`.
- Controllers de pedidos e handlers correspondentes nas camadas API e Application dos dois módulos.

## 2. Decisões de negócio a esclarecer com a equipe

Registrar as respostas antes de recomendar a arquitetura. Quando não houver decisão, apontar as alternativas, o impacto e uma premissa provisória explícita.

1. Cada loja contratará e receberá diretamente de seu gateway, ou a plataforma participará do recebimento? Haverá comissão ou divisão de valores entre recebedores?
2. Quais formas de pagamento entram na primeira entrega: Pix, cartão, boleto ou outro conjunto? Parcelamento faz parte do escopo?
3. Quem envia o produto: a loja, um fornecedor ou ambos? Há uma origem por loja ou múltiplas origens? Retirada e produtos digitais precisam ser atendidos?
4. Uma compra no Drop continua apenas como `DropOrder` ou também gera um pedido no Store? Qual registro representa a venda para cobrança, nota, envio e ERP, evitando duplicidade?
5. Quem controla estoque e reservas? Como a venda no Store, a reserva no Drop e uma possível venda em outro canal afetam a mesma disponibilidade?
6. Já existe CRM ou ERP solicitado por algum cliente? Quais rotinas ele espera automatizar? Se não existir, usar casos representativos e registrar que o conector concreto permanece em aberto.
7. Quem será responsável pela emissão fiscal, e em qual momento ela entra no processo de expedição?
8. O que precisa funcionar na demonstração do TCC e o que será apenas documentado como evolução?

## 3. Roteiro de pesquisa por frente

### 3.1. Gateway de pagamento

**Pergunta central:** como cobrar e confirmar o pagamento de um pedido com rastreabilidade, inclusive quando há falhas ou demora na resposta?

Pesquisar:

- Formas de pagamento, parcelamento, experiência de checkout, redirecionamento e opções de integração por API ou componentes do provedor.
- Requisitos de cadastro da loja, credenciais, ambiente de testes e condições para habilitar produção.
- Ciclo da cobrança: criação, pendência, aprovação, recusa, expiração, cancelamento, estorno total/parcial e contestação. Distinguir estados do pagamento e do pedido.
- Autorização e captura, quando aplicáveis, e momento de confirmação da venda.
- Webhooks, isto é, notificações enviadas pelo provedor: autenticação, assinatura, repetição, ordem de chegada e consulta posterior do status.
- Idempotência, isto é, como repetir uma operação sem criar uma segunda cobrança. Investigar o caso em que o provedor aceita o pagamento, mas a resposta não chega ao nosso sistema.
- Várias tentativas de pagamento para o mesmo pedido, prazo para pagar e relação entre expiração da cobrança e expiração da reserva do Drop. Prever confirmação recebida após a reserva expirar.
- Dados exigidos do comprador e da loja; quais dados sensíveis podem ser evitados usando recursos do provedor; requisitos de segurança e eventual escopo de PCI DSS a verificar.
- Taxas, custos de recursos adicionais, prazos de recebimento e limitações relevantes, com fonte e data da consulta.

**Resultado esperado:** fluxo de pagamento proposto, tabela de estados externos versus internos, requisitos de dados e comparação de pelo menos duas opções compatíveis com o cenário definido pela equipe.

### 3.2. Integração de envios

**Pergunta central:** como calcular o frete antes da compra e acompanhar a entrega após a aprovação do pedido?

Pesquisar:

- Integração direta com transportadora ou por intermediador de logística; cobertura, modalidades e condições de contratação.
- Dados para cotação: origem, destino, peso, dimensões, unidades, embalagem, volumes, quantidade de itens e valor declarado. Verificar se os campos atuais são suficientes.
- Diferença entre dimensões do produto e do pacote, combinação de itens e divisão de uma compra em mais de um envio.
- Cálculo de preço e prazo, validade da cotação e registro da opção escolhida no pedido.
- Contratação do envio, geração e formato de etiquetas, postagem/coleta, cancelamento e possíveis custos dessas operações.
- Documentos exigidos em cada cenário e dependência entre nota fiscal e emissão de etiqueta.
- Rastreamento por notificações ou consultas periódicas; estados de trânsito, tentativa de entrega, entrega concluída, extravio e devolução.
- Logística reversa e relação entre devolução, cancelamento e reembolso.
- Ambiente de testes, credenciais por loja, limites da API e tratamento de indisponibilidade na cotação ou na contratação.

**Resultado esperado:** fluxo de cotação até entrega, lista dos dados faltantes, proposta de estados e comparação de pelo menos duas opções compatíveis com o cenário da equipe.

### 3.3. Preparação para CRM

**Pergunta central:** quais informações comerciais o lojista precisa compartilhar com seu CRM e com qual finalidade?

Pesquisar:

- Casos de uso prioritários: cadastro de contatos, histórico de compras, segmentação e acompanhamento de relacionamento. Avaliar interesse em carrinho abandonado somente se esse fluxo fizer parte do produto.
- Quais informações seguem do e-commerce para o CRM e quais, se houver, podem voltar.
- Dados mínimos de cliente, pedido e pagamento necessários para cada finalidade; requisitos de privacidade, consentimento quando aplicável, retenção e exclusão a verificar.
- Identificação externa, duplicidade de contatos e isolamento entre lojas. Não assumir que um e-mail identifica uma pessoa globalmente na plataforma.
- Sincronização de histórico, atualizações posteriores, frequência e comportamento ao desativar a integração.
- Autenticação, permissões, limites, notificações e disponibilidade de ambiente de testes em um CRM representativo ou no sistema indicado pelo cliente.

**Resultado esperado:** matriz de dados e eventos que poderiam ser compartilhados, direção da sincronização e requisitos para ativar um futuro conector por loja. Não é necessário projetar funções internas de um CRM.

### 3.4. Preparação para ERP

**Pergunta central:** quais operações a loja quer manter no ERP e como evitar divergências com o e-commerce?

Pesquisar:

- Casos de uso: envio de pedidos aprovados, cadastro de clientes, produtos, preços, disponibilidade de estoque, faturamento e retorno dos dados fiscais/logísticos.
- Fonte principal de cada informação: quem pode alterar preço, produto, estoque, situação fiscal e status de expedição? Como resolver atualizações conflitantes?
- Identificação por SKU e IDs externos, unidades, variantes se necessárias, depósitos, reservas e vendas em outros canais.
- Particularidades de estoque do Drop: reserva temporária, liberação, confirmação de venda e concorrência com o Store e o ERP.
- Emissão e cancelamento de notas: responsabilidade do ERP, de serviço específico ou de processo externo; informações que o e-commerce precisa enviar e receber.
- Importação inicial e sincronização incremental; falhas parciais, registros duplicados, atualização fora de ordem e prevenção de ciclos de sincronização.
- Autenticação, permissões, limites, notificações e ambiente de testes em um ERP representativo ou no sistema indicado pelo cliente.

**Resultado esperado:** matriz de responsabilidade e sincronização dos dados, dependências fiscais/logísticas e requisitos para um futuro conector. Não é necessário transformar o e-commerce em um ERP.

## 4. O que avaliar como preparação do backend

Para cada item, indicar se já existe, se precisa ser criado e em qual camada/módulo. São hipóteses para orientar o desenho; não há decisão antecipada de adotar uma ferramenta ou infraestrutura específica.

| Necessidade a avaliar | Pergunta que a proposta precisa responder |
| --- | --- |
| Contratos de integração e adaptadores | Como Application usa pagamentos, envios e sincronizações sem depender dos DTOs ou SDKs de um fornecedor? O padrão de readers e adapters existente ajuda? |
| Responsabilidades Store/Drop | Qual módulo inicia a operação, recebe a confirmação e mantém o estado oficial? Como o outro módulo é atualizado sem duplicar a venda? |
| Configuração opcional por loja | Como ativar, desativar e trocar um conector, vincular a conta externa e controlar quem administra essa configuração? Como o fluxo funciona sem CRM ou ERP conectado? |
| Credenciais e autorização | Como armazenar, rotacionar e revogar segredos por loja? Como autenticar chamadas externas com permissões próprias? Não presumir que as rotas internas Store/Drop atendam terceiros. |
| Identificação e correlação | Como relacionar loja, pedido, tentativa de pagamento, envio e registro externo, inclusive após troca de fornecedor? |
| Recebimento de notificações | Como verificar origem, identificar com segurança a loja e registrar eventos sem depender da sessão de um comprador? Como tratar duplicidade e ordem de chegada? |
| Consistência e recuperação | O que acontece se o banco confirma uma alteração e a API externa falha, ou o contrário? Avaliar registro persistente de operações, tentativas, reprocessamento e eventual outbox conforme a necessidade. |
| Execução em segundo plano | Quais tarefas podem sair da requisição de compra? Avaliar worker e necessidade real de fila, sem pressupor a adoção de um broker. |
| Concorrência e limites | Como lidar com compras simultâneas, rajadas de eventos do Drop, limites das APIs e falhas prolongadas sem cobrar ou vender em duplicidade? |
| Auditoria e operação | Como acompanhar falhas, consultar o resultado externo, conciliar divergências e reprocessar com segurança? Quais dados podem aparecer em logs? Revisar o conteúdo de `GatewayPayload`. |
| Contratos de dados | Quais campos, entidades, índices, eventos, endpoints e migrações seriam necessários? Como versionar contratos sem quebrar conectores futuros? |

## 5. Como executar e documentar a pesquisa

1. **Levantar o cenário:** ler os fluxos existentes, responder às perguntas de negócio e registrar lacunas por módulo. Separar fatos observados no projeto de propostas.
2. **Consultar as opções:** priorizar documentação oficial, referência de API, sandbox, autenticação, notificações, limites e condições comerciais. Registrar URL, data e o que a fonte comprova. Quando algo não for público ou claro, marcar como pendente de confirmação.
3. **Desenhar a solução:** propor os fluxos e o conjunto mínimo de mudanças no projeto. Relacionar cada mudança a uma necessidade levantada e indicar alternativas quando houver decisão em aberto.
4. **Revisar com a equipe:** apresentar os resultados, fechar o recorte do TCC e converter as mudanças aprovadas em tarefas de implementação com dependências.

O trabalho pode ser entregue por etapas: primeiro pagamentos e envios; depois a preparação de CRM e ERP. Estimar esforço e prazo após o levantamento inicial, evitando prometer implementação antes de conhecer as APIs e dependências.

Modelo de comparação para pagamentos e envios:

| Opção | Requisitos obrigatórios atendidos | API/sandbox e autenticação | Notificações e recuperação | Conta por loja | Custos/limitações | Esforço, riscos e fontes |
| --- | --- | --- | --- | --- | --- | --- |
| A preencher | | | | | | |

Modelo de mapeamento de dados, principalmente para CRM e ERP:

| Informação/operação | Finalidade | Origem → destino | Sistema responsável | Identificação por loja | Gatilho/frequência | Conflitos e recuperação |
| --- | --- | --- | --- | --- | --- | --- |
| A preencher | | | | | | |

## 6. Entregáveis e critérios de aceite

A tarefa estará concluída quando a equipe receber e conseguir revisar:

- [ ] Cenário de negócio, premissas e decisões pendentes, incluindo recebimento por loja, responsabilidade Store/Drop e origem dos envios.
- [ ] Diagnóstico do que existe e do que falta no backend, com referências a arquivos ou fluxos do projeto.
- [ ] Comparativo de pelo menos dois gateways e duas opções de envios, com fontes oficiais, data, limitações e indicação fundamentada de uma opção por frente, se houver evidência suficiente.
- [ ] Um caso representativo de CRM e um de ERP, com dados, direção da sincronização, fonte principal de cada informação e requisitos dos futuros conectores.
- [ ] Diagrama do fluxo de venda, cobrindo cotação, pedido/reserva, pagamento, eventual faturamento, envio e sincronização. Indicar o momento de acionar cada sistema e os responsáveis.
- [ ] Mapeamento dos estados de pedido, pagamento e envio, com tratamento de cancelamento, expiração, reembolso e devolução.
- [ ] Proposta de preparação por camada/módulo, separando o necessário para o primeiro fluxo das evoluções futuras.
- [ ] Plano de validação para a implementação posterior: sucesso, recusa/expiração, timeout após operação aceita, notificação repetida ou fora de ordem, loja incorreta, pagamento após expiração de reserva, falha de cotação/etiqueta e conflito de sincronização.
- [ ] Backlog priorizado com resultado esperado, dependências, estimativa preliminar e critério de aceite para cada tarefa. Separar recorte do TCC e evolução posterior.
- [ ] Síntese para decisão da equipe: o que pode ser implementado primeiro, quais bloqueios existem e o que ainda depende de informação do cliente ou fornecedor.

Nesta tarefa, não é necessário contratar serviços, usar dados reais, operar cobranças ou envios reais, nem implementar conectores. Se uma prova de conceito for importante para responder a alguma dúvida, descrever o experimento de sandbox e o esforço como próxima atividade.

## 7. Recorte inicial proposto para discussão

| Etapa posterior à pesquisa | Resultado pretendido |
| --- | --- |
| Fundamentos do fluxo | Definir o registro oficial da venda, transições de estado, responsabilidades de estoque/reserva e configuração por loja. |
| Primeiro pagamento | Integrar um provedor e os meios escolhidos pela equipe, incluindo confirmação, prevenção de duplicidade, recuperação e cancelamento/reembolso no escopo definido. |
| Primeiro envio | Integrar uma opção de cotação e contratação, etiqueta e rastreamento, com os dados e documentos necessários. |
| Preparação para CRM e ERP | Definir contratos, identificadores, eventos e configuração opcional a partir dos casos pesquisados. Implementar cada conector quando existir demanda concreta. |

Este recorte é uma proposta para orientar a pesquisa. A decisão final sobre fornecedores, arquitetura e implementação deve ocorrer após a revisão dos resultados pela equipe.
