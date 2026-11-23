# Store — API

Referências da API Store, revisadas em 03/11/2026:

- [Endpoints comerciais](Endpoints.md): 64 rotas acrescentadas na implementação das regras do Store, entradas, permissões, respostas e exemplos; rotas antigas aparecem como contexto.
- [Autenticação e segurança](Authentication.md): JWT, cookies/CSRF, identidade de cliente e usuário, domínio da loja e matriz de acesso.
- [Aparência compartilhada](../../Shared/Appearance.md): edição, preview, publicação e manifesto público.

As rotas deste módulo começam em `/api/...` no serviço. Pelo Gateway, recebem o prefixo externo `/api/store`. Exemplo de checkout para uma loja: `/api/store/api/storefront/nome-da-loja/checkout`. A autenticação do storefront exige um Bearer de cliente da loja indicada pelo slug. [Storefront por loja](../../Shared/StorefrontPorLoja.md)

O Store já tem checkout, estoque e ciclo de pedidos. Pagamento/estorno continuam manuais, frete calculado permanece zero e NF-e não é emitida por provedor. Veja [fluxo comercial](../Application/OrdersAndCheckout.md) e [inicialização](../../Shared/StartupProject.md).
