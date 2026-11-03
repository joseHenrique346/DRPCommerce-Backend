# Store — API

Referências da API Store, revisadas em 03/11/2026:

- [Endpoints comerciais](Endpoints.md): 64 rotas acrescentadas na implementação das regras do Store, entradas, permissões, respostas e exemplos; rotas antigas aparecem como contexto.
- [Autenticação e segurança](Authentication.md): JWT, cookies/CSRF, identidade de cliente e usuário, domínio da loja e matriz de acesso.
- [Aparência compartilhada](../../Shared/Appearance.md): edição, preview, publicação e manifesto público.

As rotas deste módulo começam em `/api/...` no serviço. Pelo Gateway, recebem o prefixo externo `/api/store`; por exemplo, `/api/store/api/storefront/checkout`. A autenticação do storefront depende do host da loja e do Bearer de cliente nas operações privadas.

O Store já tem checkout, estoque e ciclo de pedidos. Pagamento/estorno continuam manuais, frete calculado permanece zero e NF-e não é emitida por provedor. Veja [fluxo comercial](../Application/OrdersAndCheckout.md) e [inicialização](../../Shared/StartupProject.md).
