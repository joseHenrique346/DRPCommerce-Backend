# Carrinho, checkout, pedidos e clientes

Revisado em 03/11/2026. Estes fluxos estão implementados no Store. Pagamento e estorno usam o adaptador manual; frete calculado continua zerado e não há emissão fiscal por provedor. O fluxo de venda do Drop ainda precisa ser conectado a essas regras.

## Componentes

Controllers enviam Commands/Queries pelo MediatR. `CartService`, `CustomerAddressService`, `CheckoutService` e `OrderLifecycleService` coordenam os fluxos com repositórios, UnitOfWork e escopos de escrita. Entidades protegem seus estados; o DbContext impede mudanças comerciais fora dos fluxos autorizados.

Contratos e rotas: [referência HTTP](../Api/Endpoints.md). Segurança: [autenticação e permissões](../Api/Authentication.md).

## Configurações da loja

`StoreSettings` pertence à Enterprise e é criado no cadastro da loja. A leitura possui fallback para os padrões quando uma loja ainda não tem o registro persistido.

| Campo | Regra atual |
| --- | --- |
| currencyCode | Código de três letras, normalizado; padrão BRL. Não representa integração automática com conversão de moedas. |
| minOrderValue | Valor mínimo sobre subtotal dos produtos; padrão zero. |
| maxItemsPerOrder | Quantidade total de unidades, opcional; quando definido, de 1 a 10.000. |
| allowBackorder | Padrão false; afeta compra e reserva. Disponibilidade física continua sendo uma informação distinta. |
| pendingOrderExpirationMinutes | De 5 a 10.080 minutos; padrão 60. |

GET/PUT `/api/store-settings` exigem `settings.manage`. Configurações públicas apropriadas aparecem também na informação da loja, sem liberar alteração ao comprador.

## Carrinho persistido

O cliente autenticado possui carrinho da própria loja. Adicionar um SKU já existente soma a quantidade; variantes distintas permanecem em itens distintos. Para produto simples, o SKU padrão pode ser resolvido sem `productVariantId`; produtos com opções exigem variante concreta.

O carrinho não reserva estoque. Leituras e alterações recalculam preços e disponibilidade atuais, considerando loja/produto/SKU/categoria, quantidade e backorder. Apenas selecionados entram nos totais da compra. Itens indisponíveis indicam decisão necessária; o cliente pode remover ou manter aguardando reposição fora da seleção.

Limites técnicos: 500 itens distintos e 10.000 unidades por item. O limite comercial de unidades por pedido e o pedido mínimo também precisam passar no checkout. A prévia de cupom no carrinho não ocupa uso.

## Endereços e clientes

`CustomerAddress` permite listar, criar, editar, escolher padrão e excluir endereços do próprio cliente. O primeiro vira padrão; excluir o padrão escolhe o mais recente restante. O checkout copia o endereço para o pedido, preservando o histórico quando o cadastro muda.

Prioridade de escolha no checkout: endereço salvo por `addressId`; ou `useProfileAddress`, que usa o padrão salvo e, se ausente, o endereço cadastral; ou campos de entrega enviados. `addressId` e `useProfileAddress=true` não podem ser combinados. Um ID de endereço de outro cliente não é aceito.

O backoffice consulta clientes por nome/e-mail/telefone, situação, UF, período e existência de pedidos. A resposta inclui quantidade de compras concluídas, total gasto e última compra; não conta qualquer rascunho como compra concluída. Endereços são consultados em `/api/customers/{id}/addresses`; pedidos, em `/api/orders/search?customerId=...`.

Os campos atuais de endereço ainda precisam ser complementados/estruturados para a integração logística e fiscal. Cadastro de endereço não equivale a cotação de frete pronta.

## Checkout

`POST /api/storefront/checkout` compra os itens selecionados do carrinho do cliente autenticado. O corpo não escolhe outro CustomerId nem informa itens/preços/desconto final como autoridade.

1. Normalizar a chave idempotente e procurar pedido da mesma loja/cliente/tentativa.
2. Conferir loja, cliente, configurações e visibilidade; abrir transação e bloquear a linha da empresa.
3. Repetir a consulta da chave após o bloqueio, evitando duas criações concorrentes.
4. Carregar selecionados, validar catálogo/SKU/quantidades/estoque, resolver endereço e calcular preços/totais no servidor.
5. Aplicar cupom, mínimo e limite de unidades; comprometer uso do cupom e reserva de estoque.
6. Criar pedido numerado e cobrança pendente, congelar itens/snapshots, remover do carrinho os itens comprados e confirmar a transação.
7. Preparar a sessão de pagamento depois da transação inicial. Hoje o resultado são instruções de pagamento manual.

Repetir a chave devolve o mesmo pedido e pode completar a preparação da cobrança que faltou. Uma nova compra usa nova chave. Falha antes do commit desfaz o lote; falha na criação da cobrança depois do commit mantém pedido/compromissos até retomada, cancelamento ou expiração. Consultar o pedido antes de repetir uma operação que ficou sem resposta.

O número comercial é sequencial por loja e é diferente do ID técnico. Nome, SKU, variante/opções, preço e endereço ficam preservados na compra. Alterações posteriores no catálogo não reescrevem o pedido.

## Ciclo de vida

```text
Pendente → Confirmado → Em processamento → Enviado → Entregue
    └──────────┴─────────────┘
           Cancelado, quando permitido
```

| Operação | Condição e efeito |
| --- | --- |
| Confirmar pagamento | Pedido aguardando pagamento; baixa reserva/estoque uma vez, captura a cobrança e preserva o compromisso do cupom. Confirmar novamente um pedido pago é idempotente. |
| Iniciar processamento | Pedido confirmado; registra histórico. |
| Enviar | Pedido em processamento. Para itens físicos exige dados de envio; retirada tem regra própria de transportadora. Registrar envio não compra etiqueta em provedor. |
| Entregar | Pedido enviado; registra entrega e histórico. |
| Cancelar pelo cliente | Somente pedido próprio ainda aguardando pagamento. Libera compromissos. |
| Cancelar pelo backoffice | Pedido pendente, confirmado ou em processamento, conforme domínio; exige orders.cancel e, se pago, orders.refund. Libera reserva ou repõe unidades baixadas, libera cupom e registra estorno quando necessário. |

Pedidos enviados/entregues não usam esse cancelamento. Devolução comercial/fiscal de mercadoria enviada não está implementada como fluxo completo.

`OrderStatusHistory` registra origem, estados anterior/novo, pagamento, motivo, instante e ator quando aplicável. O detalhe reúne itens, cobrança, envios e histórico; o cliente consulta somente os seus pedidos e não recebe toda a informação administrativa de autoria.

## Estoque, cupom e permissões

Reservas de pedidos não podem ser liberadas/efetivadas pelos endpoints genéricos de inventário. Esses endpoints retornam 409 `Inventory.OrderReservationProtected`. A confirmação identifica reserva já efetivada e não baixa novamente.

Cupom limitado é ocupado no checkout pendente. Confirmação não incrementa outra vez; cancelamento/expiração/falha definitiva liberam uma vez. Rascunho administrativo sem resgate precisa ocupar um uso livre na confirmação. Veja [cupons](Coupons.md) e [estoque](ProductsAndInventory.md).

Cancelamento administrativo valida `orders.cancel` e, quando pago, `orders.refund` depois de adquirir o bloqueio e carregar o estado atual, antes dos efeitos. A rota de leitura exige `orders.view`; confirmação e avanços exigem `orders.manage`. CRUD completo também passa pelas proteções de transição/preço e não é um atalho para fabricar pagamento.

## Expiração

O worker roda a cada minuto e seleciona pedidos com pedido/pagamento pendentes e prazo vencido. Para cada candidato, o workflow bloqueia a loja, recarrega o pedido e revalida estado e horário. Se ele foi pago nesse intervalo, não cancela nem estorna.

Quando continua vencido, cancela e libera estoque/cupom na mesma transação. O cancelamento da cobrança externa é solicitado depois do commit. Reservas avulsas possuem worker separado; ele não expira compromissos de pedidos por conta própria.

## Limites do adaptador atual

`ManualPaymentGateway` não faz chamada externa, não gera Pix real e não movimenta dinheiro. A loja confirma recebimento no backoffice; o registro de reembolso precisa ser acompanhado de devolução feita fora do sistema.

`ApplyPaymentResultCommand` é uma entrada interna preparada para resultado de gateway; sua existência não representa webhook público do Mercado Pago implementado. Antes de integrar um provedor real, implementar identificação da conta por loja, validação/consulta do pagamento, idempotência/reconciliação e estorno externo fora da transação com lock.

Frete permanece zero no cálculo atual; Invoice não transmite NF-e. Veja [integrações pendentes](../../Shared/PlanejamentoIntegracoes.md).

## Persistência

Migrations recentes: `AddStorefrontCart`, `AddCheckoutAndOrderLifecycle`, `AddOrderNumber`, `AddCustomerAddresses` e `ReserveCouponUsesAtCheckout`, além das bases de produto/estoque/cupom/configurações. Esta revisão documental não aplica migrations. Conferir o histórico do ambiente antes de usar os novos fluxos.
