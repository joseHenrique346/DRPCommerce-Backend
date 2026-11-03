# Cupons: limite de usos no checkout

Revisado em 03/11/2026. `UsedCount` representa utilizações comprometidas por pedidos pendentes de pagamento e pagos, descontadas as liberações. Não é uma contagem exclusiva de pagamentos aprovados.

## Consulta e regras de entrada

O backoffice cadastra/edita cupons pelos comandos específicos, busca por `code`, `typeId`, `isActive`, `availability` e paginação, e consulta os catálogos de tipos/operadores de condições. A ativação em `PUT /api/coupons/{id}/activation` recebe um booleano JSON puro, `true` ou `false`, diferente do objeto de ativação de produto/categoria.

A validação administrativa pode simular para um customerId da loja. A prévia pública usa o cliente autenticado quando disponível e não aceita escolher outro cliente. Nenhuma dessas prévias ocupa utilização nem autoriza o desconto final: o checkout calcula novamente sobre os itens/preços canônicos.

Consultas administrativas exigem `coupons.view`; alterações, `coupons.manage`. Contratos e rotas: [Endpoints](../Api/Endpoints.md). Fluxo de compra: [OrdersAndCheckout](OrdersAndCheckout.md).

## Compromisso e liberação

O checkout valida o cupom e compromete uma utilização antes de concluir o pedido. Incrementa `UsedCount` e cria um `CouponRedemption` ativo na mesma transação do pedido, dos itens, do estoque e da remoção dos itens do carrinho. A transação mantém o bloqueio da loja (`FOR UPDATE`); `UsedCount` também continua sendo token de concorrência.

Com `MaxUses=1`, o primeiro checkout ocupa a única utilização. Um segundo checkout com outra chave não consegue usar o cupom enquanto esse compromisso estiver ativo. Repetir a chave do primeiro checkout devolve seu pedido e não compromete outra utilização.

| Evento | Efeito no uso do cupom |
|---|---|
| Consulta de carrinho ou validação do código | Apenas calcula disponibilidade; não compromete utilização |
| Checkout concluído com desconto | Cria resgate ativo e incrementa `UsedCount` |
| Falha antes do commit do checkout | A transação desfaz o incremento e o resgate |
| Falha na criação da cobrança externa após o commit | Mantém o pedido e o compromisso até nova tentativa, cancelamento ou expiração |
| Confirmação do pagamento | Reutiliza o resgate ativo, sem novo incremento |
| Cancelamento, expiração ou pagamento recusado | Libera o resgate ativo e decrementa `UsedCount` uma única vez |

O pagamento de um checkout que já comprometeu o uso mantém o desconto aceito mesmo se o cupom expirar ou for desativado durante a espera. O limite já foi verificado quando o compromisso foi criado.

Rascunhos administrativos não comprometem utilização. Na confirmação do pagamento, um pedido administrativo sem resgate precisa encontrar uma utilização livre; não pode ocupar uma utilização comprometida por outro pedido.

O limite individual de `IsSingleUse` considera resgates ativos e os checkouts pendentes do cliente. A configuração de `MaxUses` não pode ser reduzida abaixo de `UsedCount`, que agora também inclui os pedidos aguardando pagamento.

## Dados existentes

A migration `ReserveCouponUsesAtCheckout` não altera o schema. Ela cria os resgates ausentes dos checkouts pendentes que já possuem desconto e soma apenas esses novos compromissos ao contador. Preserva os resgates e contadores existentes.

Se a versão anterior já aceitou pedidos que excedem `MaxUses`, seus descontos são preservados. O contador passa a refletir esses compromissos e novos usos ficam bloqueados; cancelamentos e expirações liberam as utilizações normalmente. A migration não cancela pedidos ou altera seus valores para eliminar o excesso anterior.

O downgrade mantém os registros de negócio para não liberar utilizações de pedidos abertos. A migration deve ser aplicada antes de disponibilizar a versão corrigida. Esta revisão documental não altera o banco; consultar o histórico do ambiente e o [guia de inicialização](../../Shared/StartupProject.md).
