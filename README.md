# DRPCommerce-Backend

Backend do TCC para loja virtual (Store) e eventos de venda com fila (Drop), em .NET 9. Store e Drop usam bancos PostgreSQL separados e são expostos por um Gateway YARP.

- [Índice da documentação](Documentation/README.md)
- [Configuração e execução local](Documentation/Shared/StartupProject.md)
- [Endpoints comerciais do Store](Documentation/Store/Api/Endpoints.md)
- [Autenticação e permissões](Documentation/Store/Api/Authentication.md)

O Store já possui categorias, variantes por SKU, estoque, cupons, carrinho, checkout, pedidos e endereços. Pagamento/estorno são manuais; Mercado Pago, Melhor Envio, emissão fiscal e o fluxo completo de venda do Drop continuam como próximas integrações.
