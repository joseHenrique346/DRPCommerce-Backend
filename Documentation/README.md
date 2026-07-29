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

Documentação da integração Store → Drop:

- [Integração HTTP com Refit](Drop/Infrastructure/StoreIntegration.md)
