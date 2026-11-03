# Casos de uso de identidade

## Fluxo

`Controller → MediatR → Validator → Handler → Repository/UnitOfWork`

Controllers de autenticação, perfil, empresas, membros, convites, propriedade, perfis de acesso, relações e domínios enviam Commands/Queries específicos. A organização segue `Features/Commands/<Responsabilidade>/<Operação>`; consultas ficam no subgrupo `Queries`, como nos demais casos de uso.

Commands/Queries têm requests e handlers próprios. DTOs de identidade ficam em `Identity/Contracts/Requests` e `Identity/Contracts/Responses`; contratos comerciais, em `Contracts`. A contagem histórica de 44 requests não descreve todos os casos de uso acrescentados ao Store.

## Exemplos para leitura

- `Authentication/LoginUser`: verifica conta/senha e cria a sessão.
- `Authentication/Refresh`: consome o refresh anterior, cria seu substituto e detecta reutilização.
- `User/UpdateUserProfile`: atualiza os campos permitidos da conta atual.
- `Enterprise/RegisterEnterprise`: cria empresa, dono principal, domínio, configurações comerciais e papéis padrão na mesma transação.
- `Invitation/AcceptInvitation`: valida destinatário, emissor, validade e perfis antes de criar o vínculo.
- `OwnershipTransfer/AcceptOwnershipTransfer`: troca a propriedade respeitando a ordem dos commits e invalida os acessos envolvidos.
- `AccessRole/SaveRole`: grava perfil/permissões e invalida acessos afetados.
- `Employee/DeactivateEmployee`: dono principal desativa funcionário e vínculo da loja na mesma operação.

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

## Relação com o fluxo comercial atual

Desativação de funcionário, cadastro da loja com configurações/papéis padrão e endereços do cliente complementam a implementação original. Convites ainda criam Membership sem onboarding automático de Employee. O domínio do cliente e sua identidade continuam sendo a autoridade para carrinho, endereço e pedido.

Registros antigos de rodadas de verificação pertenciam às reorganizações anteriores e não comprovam homologação do fluxo atual. Esta atualização de documentação não executa testes, abre servidores ou aplica migrations. A configuração vigente está em [Authentication](../Api/Authentication.md); o schema e a execução, em [Infrastructure](../../Shared/Infrastructure.md) e [StartupProject](../../Shared/StartupProject.md).

O adaptador de entrega de e-mail continua desabilitado. Confirmação/recuperação não devem expor tokens ao solicitante. Rotas, permissões e corpos atuais: [Endpoints](../Api/Endpoints.md).
