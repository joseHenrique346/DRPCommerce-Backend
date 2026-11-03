# Drop — Infrastructure

O Drop usa PostgreSQL/EF Core/Npgsql com banco e migrations próprios. Clients Refit consultam recursos do Store, e DropCommerce.Security valida sessão/tenant pelo serviço canônico.

Filtros de segurança alcançam dependentes do evento e recursos do cliente; não estão restritos à tabela de eventos. Os guards de gravação validam relações e escopos. Não há transação distribuída de compra, estoque ou pagamento pronta entre os dois bancos.

- [Integração Store → Drop com Refit](StoreIntegration.md)
- [Infraestrutura compartilhada e persistência](../../Shared/Infrastructure.md)
- [Configuração, bancos e execução](../../Shared/StartupProject.md)
