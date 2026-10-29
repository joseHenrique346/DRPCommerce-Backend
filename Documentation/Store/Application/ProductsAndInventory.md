# Produtos, variantes e estoque

O Processo D usa produto pai e SKU vendável. O catálogo, os SKUs, as reservas e as movimentações são isolados por loja. Controllers enviam Commands/Queries pelo MediatR; regras e transações ficam na Application e no Domain, com EF na infraestrutura.

## Produto e SKU

Todo produto recebe um `ProductVariant` padrão, inclusive produtos antigos. O `Product.SKU` identifica esse registro. O estoque pertence ao `ProductVariant`, permitindo que tamanho e cor tenham saldos independentes.

- Produto simples: `hasVariants=false`; o SKU padrão é vendável.
- Produto com variantes: `hasVariants=true`; o SKU padrão fica desativado e serve como referência do cadastro pai. Cadastre os SKUs concretos antes de publicar o produto na vitrine.
- Um produto simples existente pode ser convertido por `PUT /api/products/{id}/variants/enable`, desde que o SKU padrão esteja com saldo e reservas zerados. A conversão preserva os registros anteriores e não redistribui estoque automaticamente. Não há conversão inversa nesta etapa.
- SKUs são normalizados com trim e maiúsculas e são únicos por loja, considerando os produtos simples e as variantes não excluídas. O código do produto pai com variantes também fica reservado ao seu SKU padrão.
- Slugs de produto são únicos por loja e usam letras minúsculas, números e hífens.
- Uma variante concreta possui de uma a oito opções, por exemplo `Tamanho=42` e `Cor=Preto`. Todas as variantes de um produto usam os mesmos nomes de opções. A ordem das opções e diferenças de maiúsculas/minúsculas não criam uma nova combinação.
- O banco possui índices únicos por SKU e por combinação de opções. A Application também retorna conflitos específicos antes da gravação.

Exemplo de criação de variante em `POST /api/products/12/variants`:

```json
{
  "details": {
    "name": "42 / Preto",
    "sku": "TENIS-XYZ-42-PRETO",
    "barCode": "",
    "attributes": { "Tamanho": "42", "Cor": "Preto" },
    "isActive": true
  },
  "pricing": { "price": 319.90, "costPrice": 180.00 },
  "dimensions": { "weight": 0.85, "height": 12, "width": 22, "length": 34 }
}
```

`pricing` e `dimensions` são opcionais. Valores nulos de preço, custo, peso ou dimensões herdam o valor do produto pai. As respostas administrativas apresentam os valores efetivos e os overrides, permitindo identificar a herança. As respostas públicas não incluem custo, saldo reservado ou histórico interno.

Preço deve ser positivo e custo deve estar entre zero e o preço de venda. Essa foi a regra adotada para a decisão que estava aberta no planejamento. Produtos físicos exigem peso e todas as dimensões positivos; digitais podem usar zero. Valores monetários, peso e dimensões respeitam a precisão `numeric(18,2)` do banco. Alterações no pai também validam os valores efetivos das variantes, evitando custo herdado acima de um preço próprio do SKU.

Alterar as opções de um SKU, converter um produto ou mudar entre físico e digital exige saldo e reservas zerados nos SKUs envolvidos. O nome, SKU e opções de itens já vendidos são preservados no snapshot do pedido.

## Estoque e reservas

Cada SKU físico possui:

- `stockQuantity`: saldo contabilizado, antes da dedução das reservas;
- `reservedQuantity`: unidades comprometidas em reservas abertas;
- `availableQuantity`: `max(0, stockQuantity - reservedQuantity)`;
- `version`: controle de concorrência para alterações do registro;
- `tracksStock`: informa se o SKU controla estoque. Produtos digitais e o SKU padrão de um pai com variantes não controlam estoque.

`AdjustStock` recebe uma quantidade relativa, positiva para entrada e negativa para saída, com motivo obrigatório e chave de idempotência. `expectedVersion` é opcional; use-o quando o ajuste for calculado a partir de uma consulta anterior, como em uma contagem física. Uma versão desatualizada retorna 409. Repetir uma chave já concluída com a mesma quantidade e motivo retorna a movimentação anterior, mesmo que o saldo tenha mudado depois.

Exemplo em `PUT /api/inventory/variants/31/adjust`:

```json
{
  "quantityChange": 20,
  "reason": "Recebimento da nota 123",
  "idempotencyKey": "recebimento-123-sku-31"
}
```

Uma reserva agrupa até 100 SKUs distintos, com até 10000 unidades no total, respeitando também o máximo de itens configurado na loja. Todos os itens são validados antes da movimentação. A operação é integral: uma falha impede reservar parcialmente o lote.

Exemplo em `POST /api/inventory/reservations`:

```json
{
  "idempotencyKey": "checkout-abc-reserva",
  "reference": "checkout-abc",
  "items": [
    { "productVariantId": 31, "quantity": 2 },
    { "productVariantId": 32, "quantity": 1 }
  ]
}
```

Os SKUs, produtos, categoria e ancestrais precisam estar disponíveis na loja para novas reservas. O prazo usa `pendingOrderExpirationMinutes` das configurações da loja. `reference` é uma referência externa opcional, não um vínculo automático com um pedido.

O ciclo é:

```text
Reserved -> Committed
         -> Released
         -> Expired
```

- Reservar aumenta apenas o reservado.
- Liberar ou expirar reduz apenas o reservado.
- Efetivar reduz o saldo e o reservado.
- Repetir a mesma conclusão é idempotente. Liberar uma reserva expirada também é seguro.
- Uma reserva liberada ou expirada não pode ser efetivada; uma reserva efetivada não pode ser liberada.
- Reutilizar a chave de reserva com outros itens, quantidades ou referência retorna 409. Reutilizar com o mesmo conteúdo retorna a reserva existente, incluindo seu estado terminal; uma nova tentativa comercial precisa de outra chave.

Com `allowBackorder=false`, novas reservas e ajustes de saída não podem ultrapassar o estoque disponível ou comprometer o reservado. Entradas positivas permitem repor gradualmente um saldo comprometido por backorders anteriores, mesmo após desabilitar essa política. Com `allowBackorder=true`, uma reserva pode exceder o saldo; ao efetivá-la o saldo pode ficar negativo, representando unidades a repor. A baixa honra reservas já aprovadas, inclusive após uma mudança de política. `inStock` continua significando disponibilidade física; `canPurchase` considera também backorder e produtos digitais.

Um worker libera reservas expiradas a cada minuto, inclusive em lojas inativas, sem exigir contexto HTTP. Ajustes, novas reservas e conclusões também verificam expirações da loja dentro da sua transação. Uma liberação não repõe unidades de uma reserva já efetivada; devoluções físicas são entradas de estoque justificadas.

## Concorrência e auditoria

As alterações de catálogo e estoque abrem transação e usam `FOR UPDATE` na empresa. As operações participantes da mesma loja são serializadas, incluindo alterações de categoria. Isso impede que duas reservas validem simultaneamente o mesmo saldo e que uma exclusão ou conversão concorra com uma reserva. As leituras da vitrine continuam sem bloqueio exclusivo.

Os SKUs e as reservas também possuem tokens de concorrência. Índices únicos protegem chaves idempotentes, SKUs e combinações. Chaves estrangeiras compostas impedem cruzar lojas, associar uma linha de reserva a outro tenant ou ligar um item de pedido à variante de outro produto.

`StockMovement` registra entrada/saída, variação do reservado, saldos resultantes, tipo, chave da operação, motivo, SKU no momento da operação, usuário administrativo e instante UTC. A expiração automática registra usuário nulo. Movimentações e linhas de reserva são imutáveis. O DbContext impede gravações diretas de saldo, reservas e auditoria fora do escopo confiável de estoque, inclusive pelo dono principal nos CRUDs comuns.

## Exclusão e histórico

Produto e variante sempre recebem exclusão lógica. A exclusão exige saldo zero e nenhuma unidade reservada; desative o cadastro quando precisar apenas escondê-lo. O SKU padrão é gerenciado pelo produto e não pode ser editado ou excluído individualmente.

`OrderItem` passa a guardar `productVariantId`, `variantName` e `variantAttributesJson`, além dos snapshots existentes de nome, SKU e preço. Variantes distintas não são somadas no mesmo item. Para produto simples, omitir `productVariantId` seleciona o padrão; para um pai com variantes, informar um SKU concreto é obrigatório.

## Consultas e rotas

| Método e rota | Operação / permissão |
|---|---|
| POST `/api/products/add` e `/add-range` | Cadastro; `products.create`; aceita `hasVariants` |
| PUT `/api/products/update` e `/update-range` | Atualização completa; `products.edit` e, se o preço mudar, `products.change_price` |
| PUT `/api/products/{id}/details` | Detalhes e SKU padrão; `products.edit` |
| PUT `/api/products/{id}/pricing` | Preço/custo padrão; `products.change_price` |
| PUT `/api/products/{id}/dimensions` | Dimensões padrão; `products.edit` |
| PUT `/api/products/{id}/category` | Categoria/fornecedor; `products.edit` |
| PUT `/api/products/{id}/active` | Ativação; `products.edit` |
| PUT `/api/products/{id}/variants/enable` | Conversão para variantes; `products.edit` |
| DELETE `/api/products/delete/{id}` e `/delete-range` | Exclusão lógica protegida; `products.delete` |
| GET `/api/products/{productId}/variants` | SKUs administrativos; `products.view` |
| POST `/api/products/{productId}/variants` | Criação de SKU; `products.create` |
| PUT `/api/products/{productId}/variants/{id}` | Nome, SKU, código de barras, opções e ativação; `products.edit` |
| PUT `/api/products/{productId}/variants/{id}/pricing` | Overrides de preço/custo; `products.change_price` |
| PUT `/api/products/{productId}/variants/{id}/dimensions` | Overrides de dimensões; `products.edit` |
| DELETE `/api/products/{productId}/variants/{id}` | Exclusão lógica protegida; `products.delete` |
| GET `/api/products/search` | Busca administrativa; `products.view` |
| GET `/api/storefront/products/search` | Busca pública paginada |
| GET `/api/storefront/products/by-slug/{slug}` | Produto público e seus SKUs disponíveis |
| GET `/api/storefront/products` | Listagem pública anterior, mantendo resposta em array e paginação limitada |
| GET `/api/inventory/variants/{id}` | Saldo do SKU; `inventory.manage` |
| PUT `/api/inventory/variants/{id}/adjust` | Ajuste com motivo e idempotência; `inventory.manage` |
| POST `/api/inventory/reservations` | Reserva de SKUs; `inventory.manage` |
| GET `/api/inventory/reservations/{id}` | Consulta de reserva; `inventory.manage` |
| PUT `/api/inventory/reservations/{id}/release` | Liberação; `inventory.manage` |
| PUT `/api/inventory/reservations/{id}/commit` | Baixa; `inventory.manage` |
| GET `/api/inventory/movements` | Histórico paginado; `inventory.manage` |
| GET `/api/internal/products/{id}/variants` | Consulta para integrações, com a autenticação interna existente |

Busca de produtos: `term` consulta nome, marca e SKUs; `categoryId` aceita `includeSubcategories=true`; também há `brand`, `minPrice`, `maxPrice`, `inStock`, `isActive`, `sort`, `page` e `pageSize`. `isActive` é exclusivo do backoffice. Preço e disponibilidade precisam corresponder a uma mesma variante ativa. As ordenações são `price_asc`, `price_desc`, `newest` e `name`; a ordenação por preço considera o menor preço ativo do produto. As respostas públicas mostram preço mínimo/máximo e disponibilidade por SKU no detalhe. Página: 1 a 100000; tamanho: 1 a 100; a resposta paginada inclui `totalCount`.

O histórico de estoque aceita `productVariantId`, `page` e `pageSize`, com os mesmos limites de paginação. A vitrine continua respeitando a ativação da loja e a visibilidade herdada das categorias.

## Migration, validação e próximos processos

`AddProductVariantsAndInventory` cria variantes, reservas, linhas e movimentos; adiciona índices únicos e referências nos itens de pedido. Antes das mudanças, verifica slugs repetidos e SKUs repetidos após trim/maiúsculas. Havendo duplicidade, interrompe a aplicação com uma mensagem para corrigir os dados, sem escolher um cadastro para renomear.

Cada produto existente, incluindo os excluídos, recebe seu SKU padrão com estoque zero. Os códigos do catálogo são normalizados; os snapshots antigos de nome, SKU e preço permanecem intactos. Os itens antigos recebem a referência ao SKU padrão. Nenhuma quantidade é inferida a partir de pedidos antigos.

A migration foi gerada e seu SQL revisado sem aplicação ao banco. A solução compilou e o EF não apontou diferenças entre modelo e snapshot. Não foram criados ou executados testes nesta etapa, conforme solicitado.

Os comandos de estoque ficam disponíveis para a orquestração do checkout no Processo F. Nesta etapa, adicionar ou alterar um item no CRUD administrativo do pedido registra a variante e recalcula os valores, mas não reserva nem baixa estoque automaticamente. O vínculo transacional entre pedido e reserva, checkout de cliente, cancelamento/estorno do pedido e integração de venda do Drop pertencem aos próximos processos. A consulta interna já disponibiliza os identificadores dos SKUs para essa integração.
