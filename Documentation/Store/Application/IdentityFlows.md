# Casos de uso de identidade

## Fluxo

`Controller → MediatR → Validator → Handler → Repository/UnitOfWork`

Controllers de autenticação, perfil, empresas, membros, convites, propriedade, perfis de acesso, relações e domínios enviam Commands/Queries específicos. A organização segue `Features/Commands/<Responsabilidade>/<Operação>`; consultas ficam no subgrupo `Queries`, como nos demais casos de uso.

São 44 Commands/Queries, com um arquivo por request e um por handler. DTOs HTTP ficam separados em `Identity/Contracts/Requests` e `Identity/Contracts/Responses`, mantendo namespaces e campos públicos.

## Exemplos para leitura

- `Authentication/LoginUser`: verifica conta/senha e cria a sessão.
- `Authentication/Refresh`: consome o refresh anterior, cria seu substituto e detecta reutilização.
- `User/UpdateUserProfile`: atualiza os campos permitidos da conta atual.
- `Enterprise/RegisterEnterprise`: cria empresa, dono principal e domínio na mesma transação.
- `Invitation/AcceptInvitation`: valida destinatário, emissor, validade e perfis antes de criar o vínculo.
- `OwnershipTransfer/AcceptOwnershipTransfer`: troca a propriedade respeitando a ordem dos commits e invalida os acessos envolvidos.
- `AccessRole/SaveRole`: grava perfil/permissões e invalida acessos afetados.

UpdateCustomerProfile, DeactivateCustomerAccount, UpdateUserProfile e UpdateCurrentEnterprise têm nomes específicos para não se confundirem com comandos genéricos de CRUD. Os fluxos não passam a permitir gravação direta de privilégios pelo CRUD.

## Componentes comuns

IdentityHandlerBase converte falhas esperadas para Result<T> e reúne auxiliares compartilhados de conta, sessão e operação do dono principal. A lógica de cada operação está no handler correspondente, não em uma fachada delegando ao antigo serviço.

IdentityHandlerContext agrupa dependências explícitas: repositórios, UnitOfWork, identidade atual, escopo confiável, hash, JWT, opções, relógio, entrega e verificação DNS. Não usa resolução manual de serviços.

Validators de cadastro, login e troca de senha preservam as regras de entrada existentes. O pipeline mantém os códigos de identidade; os outros Commands/Queries continuam com a convenção anterior. Regras dependentes do estado do banco permanecem nos handlers e entidades.

## Persistência e concorrência

Handlers não importam EF nem recebem DbContext. Consultas simples usam predicados/projeções nos contratos; joins, agrupamentos e SQL de bloqueio ficam na infraestrutura. Consultas que atravessam filtros globais declaram essa intenção e mantêm os critérios originais de tenant/conta.

O commit de alterações e o da transação são diferentes. Rotação de refresh, persistência das falhas de login e transferência de propriedade mantêm sua ordem. Tokens consumidos são retidos durante a vida da família para detectar replay.

## Contrato HTTP

Result<T> é interno. ToIdentityActionResult mantém o DTO direto nos sucessos e ProblemDetails com status/código nas falhas. AuthControllerBase grava refresh em cookie somente após sucesso. Logout, revogação e alterações preservam seus status.

Bearer e CSRF continuam no Swagger. Envio de e-mail permanece desabilitado.

## Verificação desta reorganização

Store e Drop compilaram. Foram executadas 91 verificações sem abrir servidores ou conexões de banco: registro dos 44 handlers, dependências, validações, erros esperados, hash de senha, assinatura/rejeição de JWT adulterado, geração de SQL, Swagger e comparação dos modelos EF com os snapshots. Não foi identificada mudança de schema.

Após autorização explícita, foram executadas mais 95 verificações HTTP/de respostas com Store, Drop e Gateway nas portas HTTPS 5001, 5002 e 5000, usando duas lojas fictícias em bancos temporários isolados. Foram conferidos:

- Cadastro, login, perfil, validação de senha, CSRF e cookie de refresh com HttpOnly/Secure.
- Cadastro e seleção de lojas, perfis, permissões, convites e invalidação de tokens após mudança de acesso.
- Contas de cliente independentes por loja, inclusive com o mesmo e-mail, rejeição de tokens entre lojas e rejeição de perfil pertencente a outra loja.
- Acesso pelo Gateway ao Store e ao Drop, Swagger com Bearer e prefixo relativo.
- Transferência de propriedade, rotação de refresh, revogação da família após reutilização e logout.

A execução identificou e corrigiu uma duplicação de `traceId` na conversão de falhas para ProblemDetails. Os status e códigos de credenciais inválidas e política de senha foram conferidos por HTTP após a correção.

Para o storefront, o domínio `localhost` foi preparado exclusivamente no banco temporário e alternado entre as lojas fictícias, preservando a validação do certificado HTTPS. Isso testa a resolução de tenant e o isolamento das contas, mas não a comprovação DNS real. A verificação usou clientes HTTP; não validou a interface do navegador nem suas regras de cookies. Os limites por minuto foram elevados somente nos processos temporários para não interferirem nos testes funcionais; os limites padrão e o bloqueio progressivo não foram testados nessa execução. O envio de e-mail continua desabilitado.

As migrations existentes foram aplicadas somente aos bancos temporários, removidos ao final junto com os processos e o script auxiliar. Nenhuma migration foi aplicada aos bancos de desenvolvimento nesta reorganização, e nenhum projeto de testes foi criado.
