---
title: Store — referência dos endpoints comerciais
aliases: [Store — referência dos novos endpoints]
tags: [drpcommerce, store, api, tcc]
date: 2026-11-03
status: implementado
origin_branch: feat/89-CriacaoRegrasDeNegocioStore
baseline: 2088271b32f413048791a980a45b028c3e7e495c
code_commit: 734d537
---

# Store — referência dos endpoints comerciais

Referência dos endpoints acrescentados pelas regras do Store e integrados à `main` em `38d0230` (mesmo conteúdo funcional de `734d537`). A comparação histórica usa a base `2088271`, anterior à implementação; não é uma diferença contra a `main` atual. Foram inventariadas **64 combinações novas de método + rota**, em 17 controllers alterados. Esses controllers expõem outras **63 rotas preexistentes**, documentadas ao final para orientar o uso. Não são 127 endpoints novos. A contagem foi conferida estaticamente nos atributos HTTP e nas ações herdadas; não representa execução HTTP nem cobertura de toda a API.

[PR #91 — implementação do Store](https://github.com/joseHenrique346/DRPCommerce-Backend/pull/91) · [Issue #89 — regras do Store](https://github.com/joseHenrique346/DRPCommerce-Backend/issues/89)

## Como chamar e interpretar

- As rotas abaixo são as rotas **do serviço Store**. No Gateway atual, `/api/store` é removido antes do encaminhamento: `/api/store/api/storefront/cart` chega ao Store como `/api/storefront/cart`. Conferir o endereço publicado do ambiente.
- JSON usa propriedades em camelCase. `{id:long}` representa um inteiro de 64 bits positivo; não enviar as chaves nem `:long` na URL.
- Backoffice exige Bearer de usuário com vínculo ativo na loja e a permissão indicada. O dono principal segue as regras de autorização central. Ter um ID de outra loja não concede acesso.
- Vitrine pública resolve loja pelo host/domínio. Cliente usa Bearer de cliente daquela loja. Não enviar `enterpriseId` para escolher arbitrariamente a loja; cabeçalhos internos não são autoridade para navegador.
- Sucesso das novas rotas comerciais normalmente é HTTP **200**, com `Result<T>` e dado em `content`: `isSuccess`, `isFailure`, `content`, `errors`, `listMessageErrors`. Não presumir 201 em POST. A exceção nova é desativação de funcionário, HTTP 204.
- Erros de aplicação usam ProblemDetails, com códigos conforme a falha. Considerar 400 (validação), 401 (identidade), 403 (permissão), 404 (recurso), 409 (conflito/estoque/estado) e 503 (dependência indisponível); limites podem produzir 429. Alguns retornos de middleware/legado podem não usar o mesmo corpo.
- Não generalizar o envelope para o legado: perfil e informação da loja usam DTO direto; a listagem antiga de produtos retorna array direto; DELETE do próprio perfil retorna 204.
- Leituras paginadas usam contratos próprios; conferir `PagedResponse<T>` e os DTOs vinculados abaixo. Não depender de `get-all` para grandes catálogos.

## Índice das rotas novas

| Área | Novas rotas |
| --- | ---: |
| Categorias | 6 |
| Cupons | 5 |
| Clientes no backoffice | 2 |
| Desativação de funcionário | 1 |
| SKUs para integração interna | 1 |
| Estoque e reservas | 7 |
| Pedidos administrativos | 7 |
| Produtos | 7 |
| Variantes por produto | 6 |
| Configurações comerciais | 2 |
| Carrinho do cliente | 8 |
| Categorias da vitrine | 2 |
| Prévia pública de cupom | 1 |
| Endereços do próprio cliente | 5 |
| Checkout e cancelamento do cliente | 2 |
| Catálogo público | 2 |

## Referência por módulo

### Categorias

[Fonte: CategoryController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Category/CategoryController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/categories/tree` | `categories.view` | Consulta a árvore da loja, incluindo categorias inativas. Sem corpo; árvore de categorias. |
| `GET /api/categories/search` | `categories.view` | Busca categorias por nome, pai e situação, com paginação. Filtros na query; resposta paginada. |
| `PUT /api/categories/{id:long}/active` | `categories.manage` | Altera a ativação da categoria. Filhos e produtos de uma ramificação inativa ficam ocultos na vitrine. JSON de ativação (ver observação do módulo). |
| `PUT /api/categories/reorder` | `categories.manage` | Atualiza a ordem de exibição de várias categorias em uma transação. ReorderCategoriesCommand no corpo. |
| `DELETE /api/categories/{id:long}` | `categories.manage` | Exclui logicamente a categoria, com transferência opcional dos produtos para outra categoria ativa. Query opcional moveProductsToCategoryId. |
| `DELETE /api/categories/delete-with-products` | `categories.manage` | Exclui categorias em lote, com transferência opcional dos produtos. Subcategorias precisam integrar o lote. DeleteListCategoryCommand no corpo. |

Filtros de `search`: `name`, `parentId`, `isActive`, `rootsOnly`, `page`, `pageSize`, conforme `SearchCategoriesQuery`. A árvore administrativa inclui inativas; a pública oculta ramificações inativas. Ativação recebe `{ "isActive": true }`. Reordenação recebe `{ "items": [{ "id": 123, "displayOrder": 1 }] }`. Exclusão em lote recebe `{ "ids": [123], "moveProductsToCategoryId": null }`. Na exclusão, `moveProductsToCategoryId` permite realocar produtos; sem destino, os vínculos impedem a exclusão. Subcategorias precisam integrar o conjunto excluído. Exclusão e movimentação ficam na mesma transação.

### Cupons

[Fonte: CouponController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Coupon/CouponController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/coupons/search` | `coupons.view` | Busca cupons por código, tipo, ativação e situação (Scheduled, Active, Expired, Exhausted, Inactive), com paginação. Filtros na query; resposta paginada. |
| `GET /api/coupons/validate` | `coupons.view` | Simula o cupom sobre um carrinho, opcionalmente para um cliente da loja (condições de cliente e uso único). Query code, subTotal, itemQuantity, customerId opcional. |
| `GET /api/coupons/condition-types` | `coupons.view` | Lista os tipos de condição de cupom (valor do pedido, quantidade de itens, compras anteriores e total gasto do cliente). Sem corpo; lista de tipos de condição. |
| `GET /api/coupons/condition-operators` | `coupons.view` | Lista os operadores de condição de cupom (igual, maior que, entre, maior que e menor que, etc.). Sem corpo; lista de operadores. |
| `PUT /api/coupons/{id:long}/activation` | `coupons.manage` | Ativa ou desativa um cupom. JSON de ativação (ver observação do módulo). |

`search` recebe `code`, `typeId`, `isActive`, `availability`, `page`, `pageSize`. A disponibilidade distingue agendado, ativo, expirado, esgotado e inativo. A validação administrativa recebe `code`, `subTotal`, `itemQuantity` e `customerId` opcional; é simulação, não reserva uso. `condition-types` e `condition-operators` permitem montar condições sem fixar IDs no frontend. A ativação recebe um booleano JSON puro: `true` ou `false`.

O checkout ocupa o uso do cupom ainda no pedido pendente. Confirmação de pagamento não conta novamente; cancelamento, falha ou expiração liberam uma vez. Assim, `UsedCount` inclui usos comprometidos em pedidos pendentes e pagos. `MaxUses` impede novas ocupações quando atingido. A migration de ajuste de dados está versionada, mas não foi aplicada nesta tarefa.

### Clientes no backoffice

[Fonte: CustomerController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Customer/CustomerController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/customers/search` | `customers.view` | Busca clientes por nome, e-mail ou telefone, situação, UF, período de cadastro e histórico de compras, com paginação. Traz quantidade de compras concluídas, total gasto e data da última compra. Os pedidos de um cliente ficam em GET api/orders/search?customerId=. Filtros na query; resposta paginada. |
| `GET /api/customers/{id:long}/addresses` | `customers.view` | Lista os endereços de entrega salvos do cliente. id do cliente na rota. |

`search` aceita `term` (nome, e-mail ou telefone), `isActive`, `isVerified`, `emailConfirmed`, `stateId`, `hasOrders`, `createdFrom`, `createdTo`, `sort`, `page` e `pageSize`. Retorna dados do cliente, quantidade de pedidos concluídos, total gasto e última compra. Pedidos do cliente são consultados em `/api/orders/search?customerId=...`; não existe uma nova rota separada para isso. A consulta de endereços exige acesso administrativo à loja.

### Desativação de funcionário

[Fonte: EmployeeController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Employee/EmployeeController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `PUT /api/employees/{id:long}/deactivate` | `Dono principal + employees.manage` | Desativa o funcionário e seu vínculo com a loja, invalidando o acesso administrativo. Sem corpo; sucesso 204. |

A operação desativa o Employee e seu vínculo de acesso (Membership), revogando acesso anterior. Sucesso retorna HTTP **204**, sem envelope. A restrição ao dono principal também é aplicada no fluxo da operação. O convite existente cria Membership; o onboarding automático que criaria Employee a partir do convite continua pendente.

### SKUs para integração interna

[Fonte: InternalProductController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/InternalProduct/InternalProductController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/internal/products/{id:long}/variants` | `Serviço interno` | Consulta os SKUs e opções do produto para integrações autenticadas da loja. id do produto na rota; lista de SKUs. |

Uso exclusivo entre serviços: `X-Service-Key` e loja resolvida pelo contexto interno confiável (`X-Enterprise-Id`). Embora o controller tenha `AllowAnonymous`, o resolvedor autentica o serviço antes da autorização MVC. Não disponibilizar essa rota como API pública de navegador. A resposta inclui informações administrativas de SKUs; o Drop pode usá-la para selecionar variantes, mas ela não cria reserva nem pedido.

### Estoque e reservas

[Fonte: InventoryController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Inventory/InventoryController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/inventory/variants/{id:long}` | `inventory.manage` | Consulta estoque, reservado, disponível e versão do SKU. id do SKU na rota; saldo e versão. |
| `GET /api/inventory/movements` | `inventory.manage` | Consulta o histórico de ajustes, reservas, baixas e liberações. Query productVariantId, page, pageSize. |
| `PUT /api/inventory/variants/{id:long}/adjust` | `inventory.manage` | Registra entrada ou saída manual com motivo, idempotência e versão opcional. AdjustStockRequest; saldo atualizado. |
| `POST /api/inventory/reservations` | `inventory.manage` | Cria uma reserva avulsa sem efetuar a venda. ReserveStockRequest; reserva e vencimento. |
| `GET /api/inventory/reservations/{id:long}` | `inventory.manage` | Consulta o estado, vencimento e itens da reserva. id da reserva; estado e linhas. |
| `PUT /api/inventory/reservations/{id:long}/release` | `inventory.manage` | Libera uma reserva avulsa; rejeita reservas de pedidos. Sem corpo; libera reserva avulsa. |
| `PUT /api/inventory/reservations/{id:long}/commit` | `inventory.manage` | Efetiva uma reserva avulsa; rejeita reservas de pedidos. Sem corpo; baixa reserva avulsa. |

O saldo é controlado por variante/SKU, inclusive para produto simples, que possui variante padrão. Ajuste recebe `quantityChange`, `reason`, `idempotencyKey` e `expectedVersion` opcional. Reserva recebe `idempotencyKey`, `items` com `productVariantId` e `quantity`, e `reference` opcional. Movimentações registram alterações de estoque/reserva, saldo resultante, operação e ator.

Reservar compromete disponibilidade; efetivar realiza a baixa; liberar devolve disponibilidade. Repetições são tratadas pelas chaves/estado da operação. Reservas vinculadas a pedidos são protegidas: `release` e `commit` genéricos retornam **409 / Inventory.OrderReservationProtected**. Somente o ciclo do pedido pode movimentá-las. A expiração genérica atende reservas avulsas; as de pedidos seguem o worker de pedidos. Limites da reserva avulsa: até 100 SKUs distintos, até 10.000 unidades no total e respeito ao `maxItemsPerOrder` da loja quando configurado. A busca de movimentos é paginada, com `productVariantId` opcional.

### Pedidos administrativos

[Fonte: OrderController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Order/OrderController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/orders/search` | `orders.view` | Busca pedidos por número, status, status de pagamento, cliente, cupom, período de criação e faixa de valor, com paginação. Filtros na query; resposta paginada. |
| `GET /api/orders/{id:long}/detail` | `orders.view` | Consulta o pedido com itens, cobrança, envios e histórico de status. id do pedido; OrderDetailResponse. |
| `POST /api/orders/{id:long}/confirm-payment` | `orders.manage` | Registra pagamento manual, efetiva a reserva e preserva o uso de cupom já comprometido pelo checkout; não conta uma segunda vez. ConfirmPaymentRequest opcional; detalhe atualizado. |
| `POST /api/orders/{id:long}/start-processing` | `orders.manage` | Confirmado → Em processamento. Sem corpo; detalhe atualizado. |
| `POST /api/orders/{id:long}/ship` | `orders.manage` | Em processamento → Enviado, registrando o envio (tipo, transportadora, rastreio e previsão de entrega). Os dados do envio são obrigatórios em pedidos com produtos físicos. ShipOrderRequest; detalhe atualizado. |
| `POST /api/orders/{id:long}/deliver` | `orders.manage` | Enviado → Entregue. Sem corpo; detalhe atualizado. |
| `POST /api/orders/{id:long}/cancel` | `orders.cancel (+ orders.refund se pago)` | Cancela pedidos pendentes, confirmados ou em processamento. Devolve estoque e cupom e estorna o pagamento, se houver. Exige orders.cancel e, quando o pedido está pago, também orders.refund. CancelOrderRequest (ver diferença cliente/backoffice). |

Busca por `number`, `statusIds`, `paymentStatusIds`, `customerId`, `couponId`, intervalo de criação, `minTotal`, `maxTotal`, ordenação e paginação. Detalhe reúne itens, cobrança, envios e histórico. IDs de estados/métodos são os das entidades estáticas do projeto; não presumir IDs pelos exemplos.

Fluxo operacional: pendente → confirmado após pagamento → processamento → enviado → entregue. `confirm-payment` é **confirmação manual**, ainda sem Mercado Pago. Seu corpo permite `transactionMethodId`, `gatewayReference`, `note`; método é necessário quando ainda não existe cobrança registrada. `ship` recebe `shipmentTypeId`, `carrierName`, `trackingCode`, `estimatedDelivery`, `note`; produtos físicos exigem dados de envio, respeitando exceção de retirada local.

`cancel` recebe `reason`. Exige `orders.cancel`; quando houver reembolso, exige também **orders.refund**, dentro da transação e antes dos efeitos. O gateway atual é manual, portanto o registro de estorno ainda não comprova devolução real em uma instituição. Expiração é um fluxo separado: após adquirir o bloqueio, confirma novamente que pedido e pagamento continuam pendentes e o prazo venceu. Pagamento confirmado nesse intervalo é preservado. Uma reserva já efetivada é reconhecida, sem segunda baixa.

### Produtos

[Fonte: ProductController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/Product/ProductController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/products/search` | `products.view` | Pesquisa produtos e SKUs por categoria, marca, preço, disponibilidade e situação. Filtros na query; resposta paginada. |
| `PUT /api/products/{id:long}/details` | `products.edit` | Atualiza os dados descritivos do produto e do SKU padrão. ProductDetailsRequest. |
| `PUT /api/products/{id:long}/pricing` | `products.change_price` | Altera preço e custo; exige products.change_price e respeita os valores herdados pelos SKUs. ProductPricingRequest. |
| `PUT /api/products/{id:long}/dimensions` | `products.edit` | Altera peso e dimensões padrão do produto. ProductDimensionsRequest. |
| `PUT /api/products/{id:long}/category` | `products.edit` | Move o produto para uma categoria ativa da loja. ProductCategoryRequest. |
| `PUT /api/products/{id:long}/active` | `products.edit` | Ativa ou desativa o produto sem alterar seus saldos. JSON de ativação (ver observação do módulo). |
| `PUT /api/products/{id:long}/variants/enable` | `products.edit` | Converte um produto simples em produto com variantes. O SKU padrão deve estar sem saldo ou reservas. Sem corpo; produto atualizado. |

`search` usa `ProductSearchRequest`: `term`, `categoryId`, `includeSubcategories`, `brand`, `minPrice`, `maxPrice`, `inStock`, `isActive` (administrativo), `sort`, `page`, `pageSize`. Ordenações públicas: `price_asc`, `price_desc`, `newest`, `name`. Página entre 1 e 100.000 e tamanho entre 1 e 100.

Alterações foram separadas para reduzir mudanças acidentais: detalhes, preço, dimensões, categoria, ativação. O corpo de detalhes contém `name`, `slug`, `description`, `sku`, `barCode`, `brand`, `imageUrls` (string, conforme contrato atual), `isDigital`. Preço: `price`, `costPrice`; dimensões: `weight`, `height`, `width`, `length`; categoria: `categoryId`, `supplierId`; ativação: `isActive`. Preço não pode ficar abaixo do custo. Dimensões de produto físico devem ser positivas. Slug e SKU são únicos por loja.

`variants/enable` converte o produto simples para opções; a variante padrão precisa estar sem estoque e sem reservas. Exclusão é lógica e depende de saldo/reservas zerados. Alterar preço pela atualização completa do CRUD também exige permissão de mudança de preço.

### Variantes por produto

[Fonte: ProductVariantController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/ProductVariant/ProductVariantController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/products/{productId:long}/variants` | `products.view` | Lista variantes, opções, preços efetivos e estoque do produto. Sem corpo; lista de ProductVariantResponse. |
| `POST /api/products/{productId:long}/variants` | `products.create` | Cadastra uma combinação de opções e um SKU único. CreateVariantRequest. |
| `PUT /api/products/{productId:long}/variants/{id:long}` | `products.edit` | Altera nome, SKU, código de barras, atributos e ativação. VariantDetailsRequest. |
| `PUT /api/products/{productId:long}/variants/{id:long}/pricing` | `products.change_price` | Define ou remove sobrescritas de preço e custo do SKU. VariantPricingRequest. |
| `PUT /api/products/{productId:long}/variants/{id:long}/dimensions` | `products.edit` | Define ou remove sobrescritas de peso e dimensões. VariantDimensionsRequest. |
| `DELETE /api/products/{productId:long}/variants/{id:long}` | `products.delete` | Exclui logicamente a variante, se não houver saldo ou reserva. Sem corpo; id do SKU na rota. |

Cada SKU possui atributos como tamanho e cor, saldo e ativação próprios. Todas as variantes do produto usam o mesmo conjunto de chaves de opção; de 1 a 8 opções; combinações normalizadas não se repetem. Preço e dimensões podem sobrescrever os dados do produto; valores nulos herdam do produto. A variante padrão do produto simples é administrada pelo produto.

Criação recebe `CreateVariantRequest`: `details` obrigatório; `pricing` e `dimensions` opcionais. Atualização simples recebe apenas `VariantDetailsRequest`. Exclusão requer estoque e reservas zerados.

### Configurações comerciais

[Fonte: StoreSettingsController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StoreSettings/StoreSettingsController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/store-settings` | `settings.manage` | Consulta as regras comerciais atuais da loja. Sem corpo; retorna configurações. |
| `PUT /api/store-settings` | `settings.manage` | Atualiza limites, moeda e política comercial da loja. UpdateStoreSettingsCommand no corpo. |

GET consulta e PUT atualiza `currencyCode`, `minOrderValue`, `maxItemsPerOrder` opcional, `allowBackorder`, `pendingOrderExpirationMinutes`. São regras da loja, não parâmetros de cobrança enviados pelo comprador. O checkout considera essas regras; o carrinho não reserva estoque. Ainda não há configuração de cotação de frete integrada.

### Carrinho do cliente

[Fonte: StorefrontCartController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontCart/StorefrontCartController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/storefront/cart` | `Cliente autenticado` | Consulta o carrinho. Itens sem estoque ou indisponíveis vêm com requiresDecision = true. Query opcional couponCode; carrinho atualizado. |
| `POST /api/storefront/cart/items` | `Cliente autenticado` | Adiciona um produto (ou soma a quantidade do SKU que já está no carrinho). productId, productVariantId opcional, quantity. |
| `PUT /api/storefront/cart/items/{id:long}` | `Cliente autenticado` | Altera a quantidade de um item. quantity; id do item na rota. |
| `DELETE /api/storefront/cart/items/{id:long}` | `Cliente autenticado` | Remove um item do carrinho. Sem corpo; id do item na rota. |
| `DELETE /api/storefront/cart/items` | `Cliente autenticado` | Remove vários itens do carrinho. Array JSON de IDs de itens. |
| `DELETE /api/storefront/cart` | `Cliente autenticado` | Esvazia o carrinho. Sem corpo; esvazia carrinho. |
| `PUT /api/storefront/cart/selection` | `Cliente autenticado` | Marca ou desmarca itens para a compra. Sem itemIds, aplica a todos. itemIds e selected. |
| `PUT /api/storefront/cart/items/{id:long}/await-stock` | `Cliente autenticado` | Mantém um item sem estoque no carrinho, fora da compra, aguardando reposição. Sem corpo; id do item na rota. |

Todas as operações retornam o carrinho atualizado, com totais dos selecionados, disponibilidade e avisos (`requiresDecision`, reposição). Os IDs de itens são os do carrinho, não IDs de produto. Adicionar o mesmo SKU soma quantidade. `GET` pode receber `couponCode` para prévia. `selection` recebe `itemIds` e `selected`; sem lista aplica a todos conforme disponibilidade. `await-stock` mantém o item indisponível fora da seleção de compra. Limites técnicos atuais: 500 itens distintos e 10.000 unidades por item; o limite comercial do pedido é conferido no checkout. Preços são recalculados pelo servidor.

### Categorias da vitrine

[Fonte: StorefrontCategoryController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontCategory/StorefrontCategoryController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/storefront/categories/tree` | `Público na loja resolvida` | Consulta a árvore visível da loja, ordenada por ordem de exibição. Sem corpo; árvore de categorias. |
| `GET /api/storefront/categories/by-slug/{slug}` | `Público na loja resolvida` | Consulta uma categoria pelo slug quando ela e todos os seus ancestrais estão ativos. slug na rota. |

Retorna somente categorias visíveis: uma categoria ativa com ancestral inativo também fica oculta. Consulta por slug permite montar navegação e páginas sem expor a árvore administrativa.

### Prévia pública de cupom

[Fonte: StorefrontCouponController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontCoupon/StorefrontCouponController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/storefront/coupons/validate` | `Público; cliente opcional` | Valida um código de cupom para o carrinho. Com cliente autenticado, também avalia clientes novos/recorrentes e uso único. Query code, subTotal, itemQuantity. |

Query: `code`, `subTotal`, `itemQuantity`. Pode aproveitar o cliente autenticado para condições pessoais; não aceita escolher outro `customerId`. Os totais desta chamada são apenas simulação. O checkout relê o carrinho e valida o desconto com os valores reais antes de ocupar o uso.

### Endereços do próprio cliente

[Fonte: StorefrontCustomerController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontCustomer/StorefrontCustomerController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/storefront/me/addresses` | `Cliente autenticado` | Lista os endereços de entrega do cliente (o padrão primeiro). Sem corpo; lista endereços próprios. |
| `POST /api/storefront/me/addresses` | `Cliente autenticado` | Cadastra um endereço de entrega. O primeiro endereço vira o padrão. CustomerAddressRequest. |
| `PUT /api/storefront/me/addresses/{id:long}` | `Cliente autenticado` | Atualiza um endereço de entrega. Pedidos já feitos mantêm o endereço da compra. CustomerAddressRequest; id do endereço na rota. |
| `PUT /api/storefront/me/addresses/{id:long}/default` | `Cliente autenticado` | Define o endereço padrão do checkout. Sem corpo; id do endereço na rota. |
| `DELETE /api/storefront/me/addresses/{id:long}` | `Cliente autenticado` | Exclui um endereço. Se era o padrão, o mais recente que restar vira o padrão. Sem corpo; id do endereço na rota. |

Corpo de criação/atualização: `label`, `recipientName`, `addressLine`, `city`, `stateId`, `zipCode`, `country`, `isDefault`. País omitido usa Brasil. O primeiro endereço vira padrão. Excluir o padrão escolhe o mais recente restante. Alterar ou excluir um endereço não altera o snapshot dos pedidos feitos. Só é possível acessar endereços do próprio cliente na loja atual.

### Checkout e cancelamento do cliente

[Fonte: StorefrontOrderController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontOrder/StorefrontOrderController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `POST /api/storefront/checkout` | `Cliente autenticado` | Finaliza a compra dos itens selecionados do carrinho. Repetir a mesma idempotencyKey devolve o mesmo pedido. A resposta traz a cobrança (link, código Pix ou instruções) e o prazo para pagamento. CheckoutRequest; pedido e instruções de pagamento. |
| `POST /api/storefront/orders/{id:long}/cancel` | `Cliente autenticado` | Cancela um pedido que ainda aguarda pagamento. O estoque reservado é liberado. CancelOrderRequest (ver diferença cliente/backoffice). |

O checkout compra os itens **selecionados do carrinho**. Não recebe lista de itens, preço, desconto final nem identificador de outro cliente. Recebe `idempotencyKey`, `paymentMethodId`, `couponCode`, `useProfileAddress`, `shippingAddressLine`, `shippingCity`, `shippingStateId`, `shippingZipCode`, `notes`, `addressId` opcional. Usar endereço salvo (`addressId`) e `useProfileAddress` juntos é inválido. Sem ambos, os campos de entrega são usados. Produtos físicos exigem endereço válido.

Na mesma transação são validados catálogo, SKU, quantidades, limites, cupom e reserva. A chave é gerada por tentativa de compra; repetir devolve o pedido correspondente em vez de criar outra venda. Não reutilizar uma chave para uma compra diferente. A preparação da sessão de pagamento ocorre fora da transação inicial; atualmente a implementação é manual. A resposta traz detalhes do pedido e instruções de cobrança, sem cobrança real pelo Mercado Pago.

O cliente só cancela pedido próprio ainda pendente. O corpo `reason` é opcional nessa rota. Cancelar um pedido pago é operação administrativa, sujeita às permissões de cancelamento e reembolso.

### Catálogo público

[Fonte: StorefrontProductController](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Controllers/StorefrontProduct/StorefrontProductController.cs)

| Método e rota nova | Acesso | Entrada e efeito |
| --- | --- | --- |
| `GET /api/storefront/products/search` | `Público na loja resolvida` | Pesquisa o catálogo público com filtros e resposta paginada. ProductSearchRequest na query; página de produtos. |
| `GET /api/storefront/products/by-slug/{slug}` | `Público na loja resolvida` | Consulta um produto e seus SKUs disponíveis pelo slug, incluindo opções e preços efetivos. slug na rota. |

`search` usa os filtros de produto descritos acima, sem aceitar `isActive` administrativo. Retorna paginação, produtos ativos, preços públicos e disponibilidade. Detalhe por slug inclui SKUs ativos e opções sem custo interno. Produtos em categoria/ancestral inativo não aparecem. O GET antigo de listagem continua por compatibilidade e tem resposta diferente (array direto).

## Exemplos dos principais corpos

Os IDs abaixo são ilustrativos; usar IDs existentes na loja. As chaves de operação são exemplos, não valores para repetir em compras distintas.

### Variante com tamanho e cor

`POST /api/products/123/variants`

```json
{
  "details": {
    "name": "Tênis preto 42",
    "sku": "TENIS-PRETO-42",
    "barCode": "",
    "attributes": { "Tamanho": "42", "Cor": "Preto" },
    "isActive": true
  },
  "pricing": { "price": 199.90, "costPrice": 90.00 },
  "dimensions": { "weight": 0.8, "height": 12, "width": 22, "length": 35 }
}
```

Padronizar unidades de peso/dimensões com o catálogo antes de integrar transporte (a API do Melhor Envio trabalha com kg/cm). Não converter silenciosamente dados antigos.

### Entrada de estoque e reserva avulsa

`PUT /api/inventory/variants/456/adjust`

```json
{
  "quantityChange": 10,
  "reason": "Entrada inicial do SKU",
  "idempotencyKey": "entrada-456-20261103-01",
  "expectedVersion": 1
}
```

`expectedVersion` é opcional. Quando enviado, usar a versão consultada do SKU, não o valor ilustrativo acima.

`POST /api/inventory/reservations`

```json
{
  "idempotencyKey": "reserva-avulsa-20261103-01",
  "reference": "Separação administrativa",
  "items": [{ "productVariantId": 456, "quantity": 1 }]
}
```

Não criar reserva avulsa para depois duplicá-la no checkout. O checkout gerencia a própria reserva de pedido.

### Carrinho e checkout

`POST /api/storefront/cart/items`

```json
{ "productId": 123, "productVariantId": 456, "quantity": 1 }
```

`POST /api/storefront/checkout`

```json
{
  "idempotencyKey": "compra-67c3f72e-17b1-4d66-8a37-fde748968735",
  "paymentMethodId": 1,
  "couponCode": null,
  "useProfileAddress": false,
  "shippingAddressLine": null,
  "shippingCity": null,
  "shippingStateId": null,
  "shippingZipCode": null,
  "notes": null,
  "addressId": 789
}
```

Trocar `paymentMethodId` pelo método habilitado. Na implementação atual, a resposta contém instruções manuais; não há redirecionamento funcional do Mercado Pago.

## Rotas preexistentes nos controllers alterados

### CRUD herdado: 54 rotas, sem contá-las como novas

Aplicar cada uma das nove linhas aos seis prefixos: `/api/categories`, `/api/coupons`, `/api/customers`, `/api/employees`, `/api/orders`, `/api/products` (6 × 9 = 54). Os corpos seguem os Commands/Requests específicos da entidade, não os DTOs parciais acima.

| Método | Sufixo | O que faz / entrada |
| --- | --- | --- |
| POST | `/add` | Cria uma entidade; internamente envolve o comando em lote e retorna conteúdo em lista. |
| POST | `/add-range` | Cria lote; corpo com `commands`. |
| PUT | `/update` | Atualiza uma entidade conforme o comando específico. |
| PUT | `/update-range` | Atualiza lote; corpo com `commands`. |
| DELETE | `/delete/{id}` | Exclusão lógica de uma entidade, respeitando regras de negócio. |
| DELETE | `/delete-range` | Exclusão lógica de lote; corpo é array de IDs. |
| GET | `/get-all` | Lista sem paginação; evitar em grandes conjuntos. |
| GET | `/get-by-id/{id}` | Consulta uma entidade. |
| POST | `/get-list-by-list-id` | Consulta por array de IDs; é leitura apesar do verbo POST. |

Permissões do CRUD: categorias/cupons/clientes usam `.view` em consultas e `.manage` nas alterações; funcionários usam `employees.manage`; produtos usam `products.view`, `.create`, `.edit`, `.delete` e `.change_price` quando a atualização muda preço; pedidos usam `orders.view` e `orders.manage`. Algumas operações possuem restrições adicionais no domínio (ex.: dono principal em operações sensíveis de funcionários).

O comportamento desses endpoints foi endurecido na branch: preços e snapshots dos pedidos são calculados no servidor; transições comerciais não podem ser forçadas livremente pelo CRUD; itens ficam protegidos após o bloqueio do checkout/confirmação. Exclusão de pedido depende de ele estar cancelado ou ser rascunho pendente ainda desbloqueado. Estoque exige zerar saldo e reservas antes de excluir SKU/produto.

### Outras nove rotas preexistentes

| Método e rota | Acesso e função atual |
| --- | --- |
| GET `/api/internal/products/get-by-id/{id:long}` | Serviço interno; dados básicos de produto da loja. |
| POST `/api/internal/products/get-list-by-list-id` | Serviço interno; consulta array de IDs de produtos. |
| GET `/api/storefront/me` | Cliente; perfil próprio, DTO direto. |
| PUT `/api/storefront/me` | Cliente; atualiza perfil próprio, DTO direto. |
| DELETE `/api/storefront/me` | Cliente; desativa conta própria, sucesso 204. |
| GET `/api/storefront/store` | Público; dados públicos e configurações expostas da loja, DTO direto. |
| GET `/api/storefront/orders` | Cliente; lista seus pedidos, `page` e `pageSize`. |
| GET `/api/storefront/orders/{id:long}` | Cliente; detalhe de pedido próprio, com cobrança e histórico. |
| GET `/api/storefront/products` | Público; listagem antiga paginada por query, array direto por compatibilidade. |

Controllers não alterados e todas as rotas de autenticação, aparência, convites, domínios, entidades estáticas etc. ficam fora desta contagem. Alguns handlers compartilhados (como OrderItem e cadastro de loja) mudaram regras sem criar rotas: o cadastro de loja inicializa configurações e papéis padrão; itens de pedido passam a usar preço e snapshot do servidor.

## Situação da implementação

- Esta referência descreve o código funcional de `734d537`, integrado à main por `38d0230`. A compra com provedores externos ainda não foi homologada.
- A migration `20261103024234_ReserveCouponUsesAtCheckout` está versionada; conferir o histórico de cada ambiente antes de usar a versão corrigida. Ela compromete usos pendentes existentes; eventual excesso legado é preservado e impede novos usos até haver disponibilidade.
- Pagamento/estorno continuam manuais, frete calculado permanece zerado e não há emissão de NF-e por provedor. O modelo `Invoice` não equivale a integração fiscal pronta.
- Falta o onboarding automático de Employee a partir do convite. A [issue #90](https://github.com/joseHenrique346/DRPCommerce-Backend/issues/90) trata plano/contrato e direito de acesso ao Drop; não é resolvida apenas pelas regras comerciais do Store.
- Esta atualização documental não aplica migrations, executa testes nem altera o banco. O estado de homologação/provisionamento de cada ambiente precisa ser conferido separadamente.

## Contratos e documentação relacionada

- [Produtos e variantes](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Product/ProductRequests.cs)
- [Busca de produtos](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Product/ProductSearchRequest.cs)
- [Paginação: items, totalCount, page, pageSize](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Common/PagedResponse.cs)
- [Estoque](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Inventory/InventoryContracts.cs)
- [Carrinho](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Cart/CartContracts.cs)
- [Pedidos e checkout](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Order/OrderContracts.cs)
- [Endereços e clientes](../../../DROP-Ecommerce/StoreCommerce.Application/Contracts/Customers/CustomerContracts.cs)
- [Permissões por operação](../../../DROP-Ecommerce/src/Store/StoreCommerce.Api/Security/StoreOperationFilter.cs)

- [Autenticação, domínio e permissões](Authentication.md)
- [Carrinho, checkout, pedidos e clientes](../Application/OrdersAndCheckout.md)
- [Categorias](../Application/Categories.md)
- [Produtos, variantes e estoque](../Application/ProductsAndInventory.md)
- [Cupons e compromisso de uso](../Application/Coupons.md)
- [Integração Store/Drop](../../Drop/Infrastructure/StoreIntegration.md)
- [Pendências das integrações externas](../../Shared/PlanejamentoIntegracoes.md)
