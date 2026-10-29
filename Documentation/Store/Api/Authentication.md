# Autenticação e segurança: uso do backend

Esta etapa implementa identidade e acesso no Store e sua validação no Drop, mantendo a arquitetura .NET existente. O modelo de entidades está em [Identidade](../Domain/Identity.md).

Os casos de uso de identidade/acesso pertencem a Application e são executados por Commands/Queries e handlers. Hash, JWT e integração de segurança pertencem a `StoreCommerce.Security` e `DropCommerce.Security`, com persistência em Infrastructure.Data. As primitivas comuns continuam em Shared e são compiladas pela biblioteca `Commerce.Security.Shared`. Veja [Organização dos projetos Security](../../Shared/Security.md) e [Casos de uso de identidade](../Application/IdentityFlows.md).

## O que está disponível

Cadastro/login administrativo e de clientes, seleção de loja, refresh rotativo, logout, sessões, troca de senha, perfil próprio, criação/pausa de loja, convites, perfis/permissões, gestão de memberships, transferência de propriedade, domínio próprio com verificação TXT e isolamento de tenant.

No storefront do Drop, inscrição e entrada na fila exigem cliente autenticado. Consultas de inscrições, fila, reservas, pedidos, lista de espera e notificações são limitadas ao cliente da loja.

## Organização dos controllers

Cada controller concreta tem seu próprio arquivo e pasta em `Controllers/<Nome>/<Nome>Controller.cs`, por entidade ou função. Não há agrupamento de controllers em `Controllers/Identity` nem múltiplos controllers no mesmo arquivo.

- Autenticação: `BackofficeAuth` para usuários administrativos e `StorefrontAuth` para clientes da loja.
- Convites e propriedade: `Invitation`, `InvitationAcceptance`, `OwnershipTransfer` e `OwnershipTransferAcceptance`.
- Storefront do Store: `StorefrontEnterprise`, `StorefrontProduct`, `StorefrontCustomer` e `StorefrontOrder`.
- Integração interna do Store: `InternalIdentity`, `InternalTenant`, `InternalCustomer`, `InternalEmployee`, `InternalProduct` e `InternalEnterprise`.
- Storefront do Drop: `StorefrontDropEvent`, `StorefrontDropRegistration`, `StorefrontQueueEntry`, `StorefrontDropReservation`, `StorefrontDropOrder`, `StorefrontWaitlistEntry` e `StorefrontDropNotification`.

As bases abstratas ficam em `Controllers/Base` e não representam controllers independentes no Swagger. `AuthControllerBase` compartilha cookies e CSRF; o atributo `CookieCsrfAttribute` fica separado em `Security/CookieCsrfAttribute.cs`.

As rotas, contratos e políticas de acesso foram preservados. O Swagger apenas passa a separar os grupos conforme as responsabilidades dos novos controllers; schemas continuam representando os contratos de entrada e saída.

## Configuração obrigatória

Configure segredos por variáveis de ambiente, user-secrets ou outro armazenamento privado. Os arquivos versionados deixam as chaves vazias propositalmente; as APIs não iniciam sem SigningKey válida, e o Drop exige InternalServiceKey.

| Configuração / variável | Uso |
|---|---|
| STORECOMMERCE_CONNECTION_STRING | Banco do Store |
| DROPCOMMERCE_CONNECTION_STRING | Banco do Drop |
| STORECOMMERCE_API_BASE_URL | URL interna do Store, vista pelo Drop |
| Security__SigningKey | Chave aleatória com pelo menos 32 bytes, igual nos dois serviços |
| Security__Issuer | Emissor igual nos dois serviços; padrão DRPCommerce |
| Security__InternalServiceKey | Segredo distinto, pelo menos 32 caracteres, igual nos dois serviços |
| Security__StorefrontBaseDomain | Domínio controlado pela plataforma para subdomínios das lojas |
| Security__BackofficeUrl | URL administrativa para futura entrega de ações |
| Security__AllowedOrigins__0, __1, ... | Origins exatas de frontends autorizados no CORS |
| Security__TrustedProxies__0, __1, ... | IPs exatos dos proxies confiáveis |
| Security__RequireConfirmedEmail | false nesta etapa sem envio de e-mail |

Para desenvolvimento, `stores.localhost` pode servir como base, por exemplo `loja-a.stores.localhost`. Configure resolução DNS/hosts e HTTPS apropriados. O cadastro de loja não cria DNS ou certificados automaticamente.

Cookies de refresh e CSRF são Secure. Use HTTPS também no desenvolvimento. Não desabilite validação TLS nos clients de integração. O script de verificação aceita o certificado local somente em seu cliente de dados fictícios.

## Frontend: sequência de autenticação

1. Faça GET `/api/auth/csrf` ou `/api/storefront/auth/csrf` com `credentials: "include"`.
2. Guarde `requestToken` em memória. O servidor também configura o cookie CSRF HttpOnly.
3. Envie POST de cadastro/login com `X-CSRF-TOKEN: requestToken` e `credentials: "include"`.
4. Guarde o accessToken em memória e envie `Authorization: Bearer <accessToken>` nas operações protegidas.
5. O refresh fica somente no cookie HttpOnly; não é retornado no JSON.
6. Antes de refresh, obtenha CSRF e execute POST `refresh`, com os cookies e sem Bearer expirado.
7. Substitua accessToken/CSRF em memória ao mudar a identidade autenticada. O GET de CSRF e a operação seguinte devem usar o mesmo contexto: ambos sem Bearer, ou ambos com o mesmo Bearer válido.

Serialize as renovações no frontend, inclusive entre abas. Duas requisições usando o mesmo refresh token são interpretadas como reutilização; só uma consegue rotacionar, e a outra revoga a família. Não envie tokens em URLs, logs ou analytics.

### Testar CSRF pelo Swagger

No Swagger do Store, as operações que usam `CookieCsrfAttribute` e não são GET/HEAD exibem um parâmetro obrigatório de header chamado `X-CSRF-TOKEN`. Isso inclui cadastro, login, refresh, seleção de loja, logout, troca de senha, revogação de sessão e ações de e-mail dos fluxos administrativo e de cliente. GET de CSRF e listagem de sessões não exigem esse header. As ações de e-mail continuam indisponíveis nesta etapa.

1. Abra o Swagger em HTTPS e execute o GET `csrf` do fluxo desejado: `/api/auth/csrf` para administração ou `/api/storefront/auth/csrf` para cliente. O fluxo de cliente exige domínio de loja ativo e verificado.
2. Copie somente o valor de `requestToken` retornado. O navegador guarda o cookie correspondente.
3. Abra a operação, clique em `Try it out` e preencha o campo `X-CSRF-TOKEN` com esse valor, sem prefixo Bearer.
4. Preencha o body, quando existir, e clique em `Execute` no mesmo host HTTPS e contexto de autenticação usados para obter o CSRF.

O campo é documentação OpenAPI; não muda contratos dos controllers nem desabilita a validação no servidor. O Swagger do Drop não recebe esse parâmetro porque suas operações atuais não usam o filtro CSRF.

### Usar Bearer pelo Swagger

Os Swaggers do Store e do Drop possuem o botão `Authorize`, com o esquema HTTP Bearer JWT. Essa configuração apenas permite enviar o header de autenticação; não altera as políticas de acesso do backend. O cadastro, o login e a obtenção de CSRF continuam públicos, mesmo que o Swagger exiba o cadeado.

1. Com o `Authorize` sem token, obtenha o CSRF e faça o login no Store, preenchendo `X-CSRF-TOKEN` conforme a seção anterior.
2. Copie somente o `accessToken` retornado pelo login.
3. Clique em `Authorize`, cole o valor sem escrever `Bearer` e confirme. O Swagger monta `Authorization: Bearer <accessToken>` automaticamente nas chamadas seguintes.
4. Para operações protegidas que também exigem CSRF, execute novamente o GET `csrf` depois de autorizar e use o novo `requestToken`, mantendo o mesmo Bearer nas duas chamadas.
5. Para testar o Drop, abra seu Swagger e autorize nele também, usando o access token do Store correspondente ao fluxo administrativo ou de cliente. A autorização não é compartilhada entre as duas telas.

O refresh token permanece exclusivamente no cookie HttpOnly; não deve ser colocado no `Authorize`. Antes de renovar um access token expirado, clique em `Logout` dentro do diálogo `Authorize` para retirar o Bearer do Swagger (isso não encerra a sessão no backend), obtenha um novo CSRF sem Bearer e execute `refresh`. Depois, autorize com o novo access token e obtenha outro CSRF para as operações autenticadas. Ao selecionar outra loja, também substitua o Bearer pelo access token retornado e renove o CSRF.

Os cookies são `__Host-DRP-BackofficeRefresh` e `__Host-DRP-StorefrontRefresh`, com HttpOnly, Secure, SameSite=Strict e Path=/, sem Domain. São vinculados ao host. Para lojas com domínio próprio, publique as rotas storefront sob o domínio da loja por proxy; uma API central em outro site não é compatível com esses cookies Strict sem rever o contrato.

## Backoffice: /api/auth

| Método / sufixo | Contrato | Acesso |
|---|---|---|
| GET csrf | devolve requestToken/headerName | Público |
| POST register | fullName, email, password | Público + CSRF |
| POST login | email, password | Público + CSRF |
| POST refresh | sem body | Cookie + CSRF |
| POST select-enterprise | membershipId | Cookie + CSRF |
| POST logout | sem body | Cookie + CSRF |
| POST logout-all | sem body | User autenticado + CSRF |
| GET sessions | famílias de sessão próprias | User autenticado |
| DELETE sessions/{familyId} | sem body | User autenticado + CSRF |
| POST change-password | currentPassword, newPassword | User autenticado + CSRF |

Senha de 12 a 128 caracteres. O hash é produzido pelo servidor, nunca recebido do frontend.

Login devolve `accessToken`, `accessTokenExpiresAt`, `requiresEnterpriseSelection` e `enterprises`. Uma membership ativa é selecionada automaticamente quando é a única disponível; com várias, o token inicial é global e a seleção determina a loja. Uma conta sem loja pode criar sua primeira empresa.

`POST select-enterprise` usa membership pertencente ao dono do cookie, validada no banco. Seu ID não é um tenant arbitrário.

## Storefront: /api/storefront/auth

Mesmas operações de csrf, register, login, refresh, logout, logout-all, sessions e change-password, mas com audience `storefront` e contexto de cliente.

Cadastro recebe fullName, email, password, phone, addressLine, city, stateId, zipCode, country, gender e dateOfBirth. Não recebe EnterpriseId: a loja é resolvida pelo Host verificado e ativo.

Não há seleção de lojas para o comprador. O cliente da loja A não acessa a loja B com o mesmo JWT, nem usa endpoints administrativos.

Rotas adicionais do Store:

| Rota | Acesso |
|---|---|
| GET /api/storefront/store | Contato/nome da loja do Host, público |
| GET /api/storefront/products?page=1&pageSize=20 | Produtos ativos, sem preço de custo, público |
| GET/PUT/DELETE /api/storefront/me | Perfil do cliente autenticado |
| GET /api/storefront/orders | Pedidos próprios |
| GET /api/storefront/orders/{id} | Pedido próprio; outro cliente recebe 404 |

PUT de perfil não aceita e-mail, senha, status ou privilégio. Troca de senha usa seu caso de uso específico.

## Loja e gestão de acesso

| Rota | Operação / regra |
|---|---|
| GET /api/users/me | Conta administrativa própria |
| PUT /api/users/me | Atualiza somente fullName |
| DELETE /api/users/me | Desativa conta; protegido se ainda dona de loja |
| GET /api/enterprises/mine | Memberships próprias, incluindo lojas pausadas |
| POST /api/enterprises/register | Cria Enterprise + dono principal + subdomínio, atomicamente |
| PUT /api/enterprises/current | Dados da loja; somente dono principal |
| PUT /api/enterprises/{id}/activation | Body booleano; verifica propriedade no banco, inclusive para reativar loja |
| GET /api/enterprise-memberships/get-all | Membros da loja, dono principal |
| PUT /api/enterprise-memberships/{id} | kindId 2/3 e isActive; não rebaixa/remove dono principal |
| PUT /api/enterprise-memberships/{id}/access-roles | accessRoleIds da mesma loja |
| PUT /api/employees/{id}/deactivate | Dono principal; desativa funcionário e Membership da loja atual e invalida o acesso |
| GET /api/permissions/get-all | Catálogo estático de permissões |
| GET /api/access-roles/get-all | Perfis da loja |
| POST /api/access-roles/add | name, description, isActive, permissionIds |
| PUT /api/access-roles/{id} | Substitui configuração e invalida acessos afetados |
| DELETE /api/access-roles/{id} | Somente perfil sem vínculos; desative perfis em uso |
| PUT/DELETE /api/access-role-permissions/{roleId}/{permissionId} | Concede/remove permissão |
| PUT/DELETE /api/membership-access-roles/{membershipId}/{roleId} | Atribui/remove perfil |

Sócio-proprietário não pode administrar esses acessos. Módulos comerciais ainda não mapeados explicitamente ficam restritos ao dono principal, por padrão.

## Convites e propriedade: entrega manual

POST `/api/invitations` recebe email, kindId 2/3 e accessRoleIds. Apenas o dono principal recebe id, token e expiresAt na emissão. GET lista convites sem token; DELETE `/{id}` revoga. Nova emissão para o mesmo e-mail revoga a anterior.

O destinatário cria sua conta administrativa e faz POST `/api/identity/invitations/accept` com token e Bearer próprio. E-mail da conta deve corresponder ao convite. O uso é único, os perfis devem continuar válidos e o emissor deve continuar dono principal.

POST `/api/ownership-transfers` recebe recipientMembershipId e currentPassword. O destinatário usa POST `/api/identity/ownership-transfers/accept` com token, currentPassword e seu Bearer. DELETE `/api/ownership-transfers/{id}` cancela. A aceitação troca os tipos e invalida os tokens dos dois membros.

Não há envio automático de links nesta etapa. Entregue os tokens por canal privado; não grave o valor em listas, logs ou repositório.

## Domínio próprio

1. Dono principal faz POST `/api/enterprise-domains/add` com host, sem protocolo ou caminho.
2. Recebe recordName e recordValue para publicar no DNS como TXT.
3. POST `/api/enterprise-domains/{id}/verify` consulta DNS-over-HTTPS no resolver fixo.
4. PUT `/{id}/publication` recebe isPrimary/isActive e publica somente domínio verificado.
5. Para trocar domínio primário, marque o novo como primário/ativo; o anterior é rebaixado na mesma transação.
6. DELETE `/{id}` não exclui o domínio primário.

A verificação TXT prova controle do nome. DNS de roteamento, reverse proxy e certificado HTTPS continuam sendo configuração de infraestrutura.

## Matriz de operações comerciais

| Módulo | Leitura | Escrita |
|---|---|---|
| Produtos / variantes | products.view | products.create, products.edit ou products.delete |
| Categorias | categories.view | categories.manage |
| Cupons | coupons.view | coupons.manage |
| Fornecedores | suppliers.manage | suppliers.manage |
| Configurações da loja | settings.manage | settings.manage |
| Preço/custo existente | — | products.change_price nas rotas próprias; no CRUD completo também exige products.edit |
| Estoque / reservas / movimentações | inventory.manage | inventory.manage |
| Pedidos / itens | orders.view | orders.manage |
| Cancelamento de pedidos pelo CRUD | — | orders.manage + orders.cancel |
| Reembolso de pedidos ou do pagamento pelo CRUD | — | orders.manage + orders.refund |
| Clientes administrativos | customers.view | customers.manage |
| Funcionários / cargos / departamentos | employees.manage | employees.manage |
| Eventos/produtos/cupons Drop | drop.events.view | drop.events.manage |
| Pedidos Drop | orders.view | orders.manage |
| Inscrições/fila/reservas/lista de espera/notificações Drop administrativas | drop.events.manage | drop.events.manage |
| Outros módulos | Dono principal | Dono principal |

Audit logs do Drop não aceitam mutação manual, inclusive pelo dono. Permissões não são novos códigos arbitrários cadastrados pela loja: a loja configura perfis usando o catálogo reconhecido pelo backend.

`inventory.manage` protege os endpoints de estoque, reservas e movimentações. As rotas e regras estão em [Produtos, variantes e estoque](../Application/ProductsAndInventory.md). Cancelamento e reembolso continuam usando o CRUD existente, com verificação das permissões específicas, inclusive reembolso parcial/total do pagamento; atualizar outros campos de um pedido já cancelado ou reembolsado não exige novamente essas permissões.

A desativação de funcionário segue o fluxo de identidade reservado ao dono principal: bloqueia o dono principal como alvo, valida funcionário e Membership na loja atual e grava as duas desativações em uma transação. A Membership incrementa `AccessVersion`, fazendo os JWTs anteriores serem rejeitados na próxima validação. A conta global do usuário e os vínculos em outras lojas continuam disponíveis. O endpoint retorna 204 no sucesso, 404 para funcionário fora da loja atual ou inexistente e 409 para tentativa de desativar o dono principal.

As nove novas permissões são incluídas por migration no catálogo. Perfis existentes precisam receber as permissões desejadas pelo fluxo de gestão de acessos; permissões de produto deixam de autorizar operações de categoria.

Validação desta etapa: solução compilada e 145 verificações com dependências simuladas e consultas interceptadas, sem banco ou servidor HTTP. Foram conferidos isolamento entre lojas, proteção do dono principal, ordem da transação e encerramento sem commit em falhas, incremento de `AccessVersion`, matriz de permissões, proteção de status de pedidos e consistência entre modelo EF, snapshot e seed da migration. A migration foi gerada e não foi aplicada ao banco. O verificador temporário foi removido após a execução.

## Prazos e limites padrão

- Access token: 10 minutos, com validação de estado no banco em toda requisição protegida.
- Família de refresh: 7 dias de expiração absoluta.
- Convite: 48 horas; transferência: 30 minutos.
- Tokens de confirmação/recuperação preparados: 30 minutos.
- Login: a partir de 5 falhas, bloqueio de 30 segundos; novas falhas após espera aumentam até 15 minutos. Contadores persistidos por conta; clientes são independentes por loja.
- Janela deslizante por IP/política: auth 15/min, refresh 30/min e rotas storefront api 180/min, seis segmentos. Não é limitador distribuído.
- Retenção após expiração: 7 dias; limpeza periódica a cada 6 horas. Tokens consumidos são mantidos durante a vida da família para detectar replay.

Esses valores são configuráveis em Security. Várias réplicas precisam de limitação distribuída e chaves de Data Protection compartilhadas para cookies CSRF; isso não foi configurado como infraestrutura de produção nesta etapa.

## Proxy e integração interna

Store e Drop validam JWT com audiences diferentes para administração e cliente. O Drop também chama o Store para conferir a sessão atual; se o Store estiver indisponível, não libera acesso.

Não exponha `/api/internal` no proxy público. As chamadas exigem X-Service-Key; consultas de recurso exigem também X-Enterprise-Id enviado por serviço confiável. Nenhum endpoint público trata esse cabeçalho como autoridade.

O Host original precisa chegar à API storefront. O middleware só aceita X-Forwarded-Host/Proto/For dos IPs listados em TrustedProxies, com um salto. Sem configuração explícita, forwarding permanece desabilitado. O Gateway existente não foi transformado em provisionador de domínios/HTTPS; revise seus caminhos, prefixos e forwarding ao publicar estas rotas.

## Migrações e verificação

A Store migration `20261008194931_IdentityAndSessions` protege dados legados: para antes das alterações se houver lojas, funcionários, cargos ou departamentos antigos. Uma instalação populada necessita mapa revisado de dono e importação de vínculos/credenciais.

O schema do Drop não exigiu migration nesta etapa. Nenhuma migration foi executada nos bancos existentes do desenvolvedor.

A validação integrada foi executada durante a implementação, usando bancos temporários e dados fictícios, sem criar projeto de testes. O script temporário foi removido após a validação e não faz parte do repositório.

Rodada validada nesta entrega: 89 verificações, incluindo isolamento entre lojas/clientes, CSRF, permissões, alteração de preço, convites, transferência, revogação imediata, refresh simultâneo, bloqueio de login, rate limiting e Store indisponível. A compilação e a comparação dos modelos EF com as migrations também passaram.

O registro de 89 verificações pertence à implementação anterior. Na reorganização para handlers/repositórios foram executadas 91 verificações sem banco/servidores, além da compilação dos módulos e da comparação dos modelos EF com os snapshots. Após autorização explícita, também passaram 95 verificações HTTP/de respostas com persistência em bancos temporários isolados, cobrindo autenticação administrativa e de clientes, seleção de loja, permissões, convites, propriedade, refresh, logout e Gateway. Os processos foram encerrados e os bancos temporários removidos; os bancos de desenvolvimento não foram alterados. O domínio localhost foi preparado como fixture temporária para o storefront, sem desabilitar TLS; DNS real, interface do navegador, limites padrão e bloqueio progressivo não foram verificados nessa execução. Detalhes em [Casos de uso de identidade](../Application/IdentityFlows.md).

## Limites explícitos desta etapa

- Envio de e-mail foi adiado. Rotas request-password-reset, reset-password, request-email-confirmation e confirm-email retornam 503; não expõem tokens ao solicitante.
- RequireConfirmedEmail permanece false para permitir o desenvolvimento sem entrega. Não representa comprovação de titularidade do e-mail; habilite um fluxo seguro antes de usar isso em produção.
- Não há recuperação administrativa de proprietário por operador da plataforma, SSO ou SuperUser global.
- Integração de navegador, DNS real, HTTPS/proxy de produção e funcionamento em várias réplicas precisam de validação no ambiente de deploy.
- A etapa de identidade/autorização não criou motor de pagamento, processamento automático da fila ou um novo checkout comercial. O módulo de estoque foi acrescentado no [Processo D](../Application/ProductsAndInventory.md). Os CRUDs administrativos não são endpoints de checkout do cliente; novos casos de compra deverão usar o contexto autenticado e os preços canônicos.
- Migração de dados legados e expansão da matriz comercial dependem de decisões sobre os dados/operações reais, sem concessão ampla de acesso por padrão.

Referências técnicas: [JWT no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-9.0), [CSRF](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-9.0), [rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-9.0), [rotação de refresh tokens](https://www.rfc-editor.org/rfc/rfc9700.html#section-4.14).
