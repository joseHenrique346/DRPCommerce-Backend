# Documentação

Revisado em 03/11/2026. As referências descrevem o código integrado do projeto; propostas de integrações permanecem identificadas como pendentes.

A documentação está organizada por bounded context e por camada:

```text
Documentation/
├── Drop/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
├── Store/
│   ├── Api/
│   ├── Application/
│   ├── Domain/
│   └── Infrastructure/
└── Shared/
```

- `Drop/`: regras e componentes exclusivos do módulo DropCommerce.
- `Store/`: regras e componentes exclusivos do módulo StoreCommerce.
- `Shared/`: arquitetura e procedimentos que envolvem os dois módulos ou o Gateway.

Documentos compartilhados:

- [Infraestrutura](Shared/Infrastructure.md)
- [Startup do projeto](Shared/StartupProject.md)
- [Organização dos projetos Security](Shared/Security.md)
- [Aparência versionada, presets e contratos do frontend](Shared/Appearance.md)
- [Integrações externas — situação e próximos passos](Shared/PlanejamentoIntegracoes.md)

Identidade e autorização:

- [Modelo de usuários e lojas](Store/Domain/Identity.md)
- [Autenticação, contratos HTTP e configuração](Store/Api/Authentication.md)

Regras e contratos comerciais do Store:

- [Endpoints novos: entradas, acesso, respostas e exemplos](Store/Api/Endpoints.md)
- [Carrinho, checkout, pedidos, clientes e endereços](Store/Application/OrdersAndCheckout.md)
- [Categorias](Store/Application/Categories.md)
- [Produtos, variantes e estoque](Store/Application/ProductsAndInventory.md)
- [Cupons e compromisso de utilização](Store/Application/Coupons.md)

Índices por módulo:

- Store: [API](Store/Api/README.md), [Application](Store/Application/README.md), [Domain](Store/Domain/README.md), [Infrastructure](Store/Infrastructure/README.md).
- Drop: [API](Drop/Api/README.md), [Application](Drop/Application/README.md), [Domain](Drop/Domain/README.md), [Infrastructure](Drop/Infrastructure/README.md).

Documentação da integração Store → Drop:

- [Integração HTTP com Refit](Drop/Infrastructure/StoreIntegration.md)

O Store já executa checkout e workflow de pedidos. O Drop possui entrada na fila e consultas, mas admissão, reserva canônica e venda integrada continuam pendentes. Pagamentos/estornos são manuais, frete calculado está zerado e não há provedor fiscal implementado.
