# Drop — API

O Drop possui API própria, banco separado e autenticação validada pelo Store. As rotas do serviço recebem `/api/drop` como prefixo externo no Gateway. Revisado em 20/11/2026.

## Superfície atual

- Administração de eventos, produtos, cupons e recursos do Drop por controllers/CRUDs com permissões.
- Storefront em `/api/storefront/{storeSlug}/drop`: eventos públicos, inscrição e entrada na fila; consultas privadas dos recursos do cliente.
- Aparência de padrão/evento, usando os controllers compartilhados e as regras próprias de resolução.

`GET /api/storefront/{storeSlug}/drop/events` é público na loja resolvida. `POST /api/storefront/{storeSlug}/drop/events/{id}/queue` exige cliente, janela aberta e inscrição elegível quando configurada. `GET /api/storefront/{storeSlug}/drop/queue` retorna participações próprias.

O fluxo comercial controlado agora possui catálogo próprio, admissão FIFO, prazo de quinze minutos,
reserva por variação/CPF e checkout pela conexão Mercado Pago da loja. CRUDs financeiros
legados não confirmam vendas. Consulte as rotas, os contratos e as limitações em
[Fluxo comercial DROP](../Application/FluxoComercial.md). A capacidade de milhares de compradores
ainda precisa ser medida; frontend, frete e homologação externa continuam pendentes.

- [Identidade e permissões](../../Store/Api/Authentication.md)
- [Integração interna com o Store](../Infrastructure/StoreIntegration.md)
- [Casos de uso e pendências](../Application/README.md)
- [Aparência](../../Shared/Appearance.md)
