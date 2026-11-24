# Exclusão de conta e preservação das vendas

Implementado em 17/11/2026. O commit local `04c4de2` guarda a preparação anterior das integrações; esta evolução trata a exclusão do perfil e a identificação histórica de compradores no Store.

## Contrato HTTP

| Conta | Prévia dos impedimentos | Exclusão |
| --- | --- | --- |
| Usuário global, lojista/funcionário | `GET /api/users/me/deletion` | `DELETE /api/users/me` |
| Cliente autenticado em uma loja | `GET /api/storefront/{storeSlug}/me/deletion` | `DELETE /api/storefront/{storeSlug}/me` |

As rotas existentes de DELETE passam a exigir corpo JSON com a senha atual. A alteração de contrato é intencional: a sessão aberta, sozinha, não autoriza apagar a conta.

```json
{
  "currentPassword": "senha atual da conta autenticada"
}
```

Enviar Bearer da conta correta e `X-CSRF-TOKEN`, obtido pelo fluxo CSRF do respectivo contexto. Há limite de requisições de autenticação, validação de senha e bloqueio por tentativas incorretas. A senha não entra em URL nem em representação textual do objeto de solicitação. O frontend precisa parar de enviar DELETE sem corpo.

A prévia devolve `canDelete` e `blockers`, com `code`, `message` e `count`, sem dados de outros titulares. É informativa: o DELETE repete as verificações dentro da transação. Respostas: `204` após sucesso; `400` para entrada inválida; `401` para credenciais inválidas; `403` para CSRF inválido; `409` para impedimento comercial. O sucesso apaga o cookie de refresh do contexto e invalida imediatamente as sessões, inclusive access tokens ainda dentro da validade.

## Impedimentos

- O usuário global não pode excluir o perfil enquanto constar como dono principal de qualquer loja, inclusive pausada ou com vínculo inativo. É necessário transferir a propriedade pelo fluxo existente. Excluir usuário não exclui lojas, produtos ou vendas.
- O cliente não pode apagar o perfil com pedidos pendentes, transações pendentes/autorizadas, criação de pagamento em processamento ou de resultado desconhecido, pedidos pagos ainda em atendimento, envios não finalizados ou documentos fiscais pendentes. A verificação inclui registros excluídos logicamente para não esconder pendências.
- Uma operação financeira incerta bloqueia mesmo se o pedido já tiver sido cancelado. Primeiro é necessário recuperar seu resultado. Cancelamentos/estornos em falha, revisão ou aguardando confirmação manual e notificações financeiras pendentes/em revisão também bloqueiam, conforme os fluxos de [estorno](CancelamentoEstorno.md) e [Mercado Pago](MercadoPago.md). Pagamentos manuais pendentes devem ser concluídos/cancelados pelo fluxo comercial existente.

Esses impedimentos protegem a conclusão operacional. O atendimento dos direitos do titular e pedidos de eliminação de dados desnecessários não deve ser reduzido a um bloqueio permanente da interface: o controlador precisa oferecer o processo adequado para analisar retenção e exceções.

## Dados apagados e preservados

O cadastro é substituído por um registro residual inacessível, preservando a chave estrangeira necessária às vendas e à auditoria. Nome vira um rótulo de conta excluída, e-mail vira um identificador aleatório em `deleted.invalid`, senha é removida e sessões são invalidadas. Não se usa hash do e-mail original para simular anonimização. O instante fica em `PersonalDataErasedAt`.

No cliente da loja também são removidos telefone, gênero, nascimento, endereço do perfil, endereços salvos, carrinho, tokens de ação e refresh, documentos cadastrais vinculados por FK ou referências legadas reconhecidas e o contato redundante de `PaymentAttempt`. UF e nascimento tornam-se nulos no cadastro encerrado; não se inventa local ou data de nascimento. Contas ativas continuam exigindo dados válidos nas entradas existentes.

No usuário global são apagados perfil/credenciais/tokens, retirados os acessos das lojas e perfis de permissão, desativados e desidentificados os cadastros de funcionário vinculados e revogados convites/transferências aplicáveis. Credenciais de provedores pertencentes a lojas transferidas continuam sendo recursos dessas lojas; apagar o antigo operador não apaga a conta comercial ou seus pagamentos.

Permanecem pedidos, itens, preços/descontos, pagamentos/estornos, movimentações de estoque/cupom, notas, envios, seus IDs e snapshots comerciais. O filtro de transações passa a depender da loja do pedido, para o histórico financeiro continuar acessível após a exclusão do comprador. O histórico de vendas não consulta mais o nome atual do cadastro.

`User` global e `Customer` por loja são identidades diferentes no modelo atual. A operação não procura e apaga outras contas com o mesmo e-mail, nem associa compras de uma conta antiga a uma conta nova que reutilize esse endereço. Exclusão em uma loja não exclui o cadastro de cliente em outra.

## Snapshot do comprador

Cada novo pedido guarda `BuyerName`, `BuyerEmail`, `BuyerPhone`, `BuyerSnapshotCapturedAt` e `BuyerSnapshotSource`, além do endereço já copiado para o pedido. O DbContext captura o comprador autenticado/referenciado pelo servidor em todos os caminhos de criação, inclusive administrativos. Não recebe esses dados livres no checkout do comprador.

O snapshot é imutável por operações normais. Editar ou apagar o perfil não reescreve os pedidos. A resposta de detalhe acrescenta `buyer`, e a busca administrativa usa o nome histórico.

Origem:

| Valor | Significado |
| --- | --- |
| `1 — AtOrderCreation` | Capturado ao criar um pedido novo. |
| `2 — LegacyCurrentProfile` | Pedido antigo preenchido na migração a partir do cadastro então disponível; não prova o nome exato usado no passado. |
| `3 — Unavailable` | Não havia um cadastro compatível recuperável. |

Snapshots **continuam contendo dados pessoais**. A exclusão do perfil não é anonimização completa da venda. Restrinja o acesso por loja/permissão e conserve somente o necessário para a finalidade comercial/fiscal e o prazo definido para a operação. Não se deve usar esse contato histórico como lista de marketing.

## Concorrência e banco

Exclusão do cliente, checkout e mutações do carrinho usam a ordem loja → cliente para os bloqueios. A criação administrativa de pedidos também obtém esses bloqueios antes de capturar o snapshot, abrindo uma transação local quando necessário. Uma compra que termina primeiro bloqueia a exclusão por pendência; uma exclusão que termina primeiro impede novas compras ou endereços por requisições autenticadas anteriormente. A prévia sozinha não garante que uma exclusão posterior será permitida.

O usuário global é bloqueado na mesma ordem usada pelo cadastro de lojas e aceitação de propriedade. A limpeza e o apagamento do perfil acontecem na mesma transação; falhas desfazem a operação. Senha incorreta preserva os contadores de tentativas, conforme o fluxo de autenticação existente.

A migração `AddAccountErasureAndBuyerSnapshots` deve ser aplicada antes de iniciar a nova API. Ela inclui o preenchimento dos snapshots legados. Reverter a estrutura depois de apagar dados é recusado: o schema anterior exigia campos que foram eliminados, e um rollback não pode reconstruí-los. Backup não deve ser utilizado para reativar automaticamente perfis apagados.

Validação realizada em PostgreSQL 17 temporário e isolado: 52 verificações de exclusão/snapshots e 42 verificações de regressão das integrações, além da compilação da solução. Foram exercitados senha/CSRF, invalidação de sessão real, limpeza de documentos/carrinho/acessos, pendências financeiras/fiscais, preservação das vendas e concorrência com checkout, pedido administrativo, endereço e carrinho. Os bancos existentes e contas reais não foram alterados.

## Práticas pesquisadas e limites operacionais

A implementação adota reautenticação para ação sensível, CSRF, invalidação de sessões, verificação transacional de pendências e separação entre perfil e registros comerciais. A [OWASP recomenda reautenticação para funcionalidades sensíveis](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#require-re-authentication-for-sensitive-features).

A [ANPD explica as hipóteses de conservação do art. 16 e o dever de eliminação quando elas não se aplicam](https://www.gov.br/anpd/pt-br/acesso-a-informacao/perguntas-frequentes), em especial no item 6.6. A LGPD não fornece um prazo único para todo dado do sistema. Por isso esta entrega não inventa um prazo fiscal geral nem declara conformidade integral: a política de retenção deve distinguir pedido, nota, contato, antifraude, logs e backups, com validação da operação e do responsável jurídico/fiscal.

Antes da operação real, definir e executar essa política também nos documentos e sistemas externos. `Document.FileUrl` é apenas uma referência genérica: apagar a linha não demonstra exclusão do arquivo hospedado por terceiros. Também é necessário tratar cópias em logs, backups, provedores e outros módulos. Para volumes grandes e limpeza distribuída, evoluir para solicitação durável, outbox/worker, etapas idempotentes e acompanhamento do resultado.

O escopo transacional desta entrega é a identidade e o ciclo financeiro canônico do Store. O Drop é outro serviço/banco: filas, sinais antifraude, auditoria e snapshots próprios exigem política e coordenação entre módulos. A identidade encerrada deixa de autorizar o cliente pelo Store, mas esta rota não afirma apagar dados em outro banco. A venda futura do Drop deve se vincular ao pedido Store, conforme a #92, para compartilhar os bloqueios financeiros e o snapshot da compra.

Recomendação para o produto: exibir os impedimentos antes da senha final, explicar quais registros comerciais serão conservados e por quê, oferecer o canal de atendimento e, quando disponível, exportação dos dados antes da exclusão. Nenhuma ação sobre contas reais foi executada nesta implementação.
