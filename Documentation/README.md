# Documentação

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

Identidade e autorização:

- [Modelo de usuários e lojas](Store/Domain/Identity.md)
- [Autenticação, contratos HTTP e configuração](Store/Api/Authentication.md)

Documentação da integração Store → Drop:

- [Integração HTTP com Refit](Drop/Infrastructure/StoreIntegration.md)
