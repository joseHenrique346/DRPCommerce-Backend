# Drop — API

O Drop possui API própria, banco separado e autenticação validada pelo Store. As rotas do serviço recebem `/api/drop` como prefixo externo no Gateway. Revisado em 03/11/2026.

## Superfície atual

- Administração de eventos, produtos, cupons e recursos do Drop por controllers/CRUDs com permissões.
- Storefront em `/api/storefront/drop`: eventos públicos, inscrição e entrada na fila; consultas privadas dos recursos do cliente.
- Aparência de padrão/evento, usando os controllers compartilhados e as regras próprias de resolução.

`GET /api/storefront/drop/events` é público na loja resolvida. `POST /api/storefront/drop/events/{id}/queue` exige cliente, janela aberta e inscrição elegível quando configurada. `GET /api/storefront/drop/queue` retorna participações próprias.

Existir CRUD de pedido ou reserva não significa que admissão, reserva de SKU, pagamento e venda estejam integrados. O mecanismo completo e a capacidade de milhares de compradores ainda precisam ser implementados e medidos.

- [Identidade e permissões](../../Store/Api/Authentication.md)
- [Integração interna com o Store](../Infrastructure/StoreIntegration.md)
- [Casos de uso e pendências](../Application/README.md)
- [Aparência](../../Shared/Appearance.md)
