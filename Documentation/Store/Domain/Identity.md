# Identidade, lojas e autorização

Implementação na branch `feat/83-CriacaoUsuarioLogin`. O backend agora possui casos de uso de autenticação e gestão de acesso, além das entidades e mapeamentos. Para configuração, contratos HTTP e limitações desta etapa, veja [Autenticação e segurança](../Api/Authentication.md).

## Modelo escolhido para o TCC

`Enterprise` é a loja e também o tenant. Não foi criada uma entidade Store separada. O cliente acessa uma loja por seu domínio; não há conta de comprador compartilhada entre lojas.

- `User`: conta administrativa global. Pode criar lojas e participar de outras lojas.
- `EnterpriseMembership`: liga User à loja, com tipo, status e versão de acesso.
- `EnterpriseMembershipKind`: 1 = dono principal, 2 = sócio-proprietário, 3 = funcionário.
- `Employee`: perfil funcional opcional da membership; não guarda senha.
- `Customer`: conta independente da loja, com senha própria; não tem FK para User.
- `AccessRole`: perfil de acesso configurável por loja, como Catálogo ou Financeiro.
- `Permission`: catálogo global de operações reconhecidas pelo código.
- `MembershipAccessRole` e `AccessRolePermission`: atribuição dos perfis e suas permissões.
- `EmployeePosition` e `Department`: cargo e departamento; não concedem autorização.
- `EnterpriseDomain`: hostname normalizado, verificação e publicação da loja.
- `EnterprisePlan`: plano contratado pela loja (entidade estática), com as capacidades que ele libera.

```text
User → EnterpriseMembership ← Enterprise → Customer
                  ↓                ↓
            MembershipAccessRole  EnterpriseDomain
                  ↓
              AccessRole → AccessRolePermission ← Permission

EnterpriseMembership → Employee → EmployeePosition / Department
```

O mesmo e-mail pode existir em clientes de lojas diferentes, com senhas diferentes. Um User também pode ter o mesmo e-mail de um Customer: são identidades e públicos de token distintos.

## Autoridade e permissões

Somente o dono principal gere convites, tipos de membership, perfis, permissões e transferência de propriedade. Sócios e funcionários usam os perfis atribuídos e não recebem essa autoridade pelo nome do cargo ou por uma permissão operacional.

O dono principal possui acesso operacional implícito dentro de sua loja. Não existe `SuperUser` global. A transferência exige senha do dono atual, destinatário ativo da mesma loja, aceite autenticado e senha do destinatário. O antigo dono vira sócio; a transação mantém exatamente um dono principal ativo.

Mudar permissões, perfis, tipo ou status de membership invalida os JWTs administrativos afetados por `AccessVersion`. Alterar senha, encerrar todas as sessões ou desativar uma conta invalida as sessões por `SecurityVersion`. Essas versões são verificadas no banco a cada requisição protegida.

## Planos por contrato

O contrato comercial é da loja, não do usuário. `Enterprise` passou a carregar `PlanId`, e o dono herda as capacidades do plano via membership. Isso mantém a regra de que o `User` é uma conta global e o que ele "compra" é representado pela loja selecionada.

`EnterprisePlan` é uma entidade estática (`plano_empresa`) com três planos e um catálogo de capacidades:

| Plano | Vitrine | Contas de funcionário | Domínio próprio | Tema personalizado | Módulo DROP |
|---|---|---|---|---|---|
| Comum (1, padrão) | Sim | Não | Não | Não | Não |
| Premium (2) | Sim | Sim | Sim | Sim | Não |
| Drop (3) | Sim | Sim | Sim | Sim | Sim |

Os códigos de capacidade (`vitrine`, `employees`, `custom_domain`, `custom_theme`, `drop`) ficam em `SharedCommerce.Application` para que Store e Drop compartilhem os mesmos valores. As flags da entidade são convertidas nesses códigos na emissão do token; o `EnterprisePlan.Default` (Comum) é usado quando o `PlanId` não corresponde a nenhum plano conhecido.

O plano é emitido no JWT como claim `enterprise_plan` (o `PlanId`) e como claims `plan_capability` (uma por capacidade). `ValidateAsync` compara o `PlanId` da claim com o plano atual da loja, então trocar de plano invalida imediatamente os tokens administrativos antigos, sem exigir novo login. O gating por capacidade não abre exceção para o dono principal: o dono só usa um recurso se o plano contratado o incluir.

A troca de plano é feita exclusivamente por `PUT /api/enterprises/current/plan`, restrito ao dono principal e validado no servidor (ver [Autenticação e segurança](../Api/Authentication.md)).

## Persistência de credenciais e ações

| Entidade | Tabela | Vínculo |
|---|---|---|
| UserRefreshToken | usuario_refresh_token | User, família de sessão e membership selecionada opcional |
| CustomerRefreshToken | cliente_refresh_token | Customer + Enterprise, família de sessão |
| UserActionToken | usuario_token_acao | User, finalidade e expiração |
| CustomerActionToken | cliente_token_acao | Customer + Enterprise, finalidade e expiração |
| EnterpriseInvitation | convite_empresa | Loja, e-mail convidado, tipo e membership emissora |
| InvitationAccessRole | convite_empresa_papel_acesso | Convite + perfis da mesma loja |
| OwnershipTransfer | transferencia_propriedade | Loja, membership emissora e destinatária |

Senhas usam PasswordHasher do ASP.NET Core. Refresh tokens e tokens de ação são aleatórios; o banco guarda somente SHA-256. Refresh é rotacionado atomicamente; reutilização revoga toda a família. A expiração absoluta da família não é prolongada por rotação.

Confirmação de e-mail e recuperação têm entidades e casos de uso preparados, mas permanecem indisponíveis enquanto a entrega segura de tokens estiver desabilitada nesta etapa. Convites e transferências têm entrega manual pelo dono principal, com token devolvido uma única vez na emissão.

## Invariantes e isolamento

- User: e-mail normalizado único globalmente.
- Customer: e-mail normalizado único por loja, inclusive após exclusão lógica.
- Membership: única por `(EnterpriseId, UserId)`.
- Índice parcial: no máximo um dono principal ativo por loja; cadastro e transferência preservam sua existência.
- Hostname único globalmente; no máximo um domínio primário por loja.
- Loja nova recebe subdomínio da plataforma, já verificado e primário.
- Domínio próprio começa não verificado e inativo; publicação depende de prova DNS TXT.
- Para desativar/excluir um domínio primário, escolha outro primário primeiro.
- Loja sempre referencia um plano existente; `PlanId` usa `Restrict` no banco e padrão Comum em lojas antigas.
- Employee: relação 1:1 opcional com membership e referências funcionais da mesma loja.
- FKs compostas de tenant protegem relações de memberships, perfis, convites, funcionários e transferências.
- Query filters e proteção de SaveChanges rejeitam dados ou referências de outra loja.
- Tenant do storefront vem do Host verificado; tenant administrativo vem da membership autenticada.
- DTO, query string ou cabeçalho público não concedem autoridade sobre um tenant.
- Operações de clientes usam CustomerId autenticado, não um ID escolhido no body.

## Camadas e fluxos

```text
Controllers de identidade
  → IIdentityService / IAccessManagementService (Application)
    → serviços transacionais (Infrastructure)
      → entidades de domínio + StoreCommerceDbContext + configurações EF
```

Os serviços de identidade usam uma mesma unidade EF/transação para bloquear conta/loja, modificar relações e consumir tokens atomicamente. Os CRUDs comerciais continuam usando MediatR, repositórios e UnitOfWork existentes. CRUD genérico não pode modificar diretamente as entidades de identidade ou autorização.

O Drop mantém banco próprio, sem FKs entre serviços. Usa os IDs canônicos do Store e valida sessão, tenant e recursos por integração interna autenticada. Veja [Integração Store/Drop](../../Drop/Infrastructure/StoreIntegration.md).

## Migration e dados antigos

Foi criada `20261008194931_IdentityAndSessions`, sem alterar as migrations históricas. Ela foi validada em banco temporário vazio.

O plano por contrato foi adicionado pela migration `EnterprisePlans`: cria `plano_empresa`, insere os três planos e liga `empresa.plan_id` com padrão Comum nas lojas existentes.

A migration interrompe antes de modificar uma instalação que tenha lojas, funcionários, cargos ou departamentos antigos. É necessário definir o dono de cada loja e revisar a importação das credenciais/vínculos antes de migrar dados reais. Nenhum dono é inventado e nenhuma senha antiga é descartada automaticamente.

Não foi aplicada migration aos bancos existentes do desenvolvedor. Não foi criado projeto de testes; a validação foi executada com bancos temporários e dados fictícios. O script temporário foi removido após a validação.
