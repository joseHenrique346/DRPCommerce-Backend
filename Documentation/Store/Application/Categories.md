# Categorias do Store

Os casos de uso ficam em `StoreCommerce.Application/Features/Commands/Category`. Controllers enviam Commands/Queries pelo MediatR; handlers usam repositórios e UnitOfWork, sem depender do DbContext.

## Regras

- A loja vem do tenant autenticado/resolvido pelo domínio da vitrine.
- A árvore admite três níveis: raiz, filha e neta. Criar um quarto nível ou mover uma ramificação que exceda esse limite retorna `Category.MaximumDepth`.
- O pai precisa existir, não estar excluído, estar ativo e pertencer à mesma loja ao criar ou trocar o pai. Editar os outros campos de uma categoria cujo pai foi desativado continua permitido.
- A alteração de pai considera a árvore final do lote e impede ciclos, inclusive entre duas alterações do mesmo lote. As entidades só são alteradas depois da validação completa.
- `slug` pode ser omitido, nulo ou vazio. Nesse caso ele é gerado a partir do nome, removendo acentos e substituindo separadores por hífens, com limite de 100 caracteres. O slug informado deve usar letras minúsculas, números e hífens simples.
- Slugs de categorias não excluídas são únicos na loja, com validação na Application e índice único filtrado no banco. Uma colisão, inclusive entre slugs gerados no mesmo lote, retorna conflito. A troca simultânea de slugs já ocupados é recusada; use um slug temporário para liberar o valor.
- Alterações da árvore e dos slugs abrem uma transação e bloqueiam a linha da empresa com `FOR UPDATE`. Isso serializa as operações de categoria da mesma loja.

## Ativação e vitrine

`IsActive` representa o estado próprio da categoria. Ela só aparece na vitrine se ela e todos os seus ancestrais estiverem ativos e não excluídos. Desativar o pai oculta a ramificação e os produtos, preservando os estados próprios dos filhos. Reativar o pai mantém ocultos os filhos que já estavam inativos.

Árvore pública, busca por slug e listagem pública de produtos compartilham essa regra. A listagem de produtos aplica a seleção de categorias visíveis antes da paginação, preservando a resposta em array e os limites anteriores de página/tamanho. Ela agora passa por Query/handler. Loja inativa retorna 404 nas consultas públicas.

O backoffice inclui categorias inativas. A árvore é ordenada por `DisplayOrder`, nome e id em cada nível. A busca permite nome parcial sem distinguir maiúsculas/minúsculas, `parentId`, `isActive` e `rootsOnly`. A resposta inclui `items`, `totalCount`, `page` e `pageSize`. A página aceita 1 a 100000 e o tamanho aceita 1 a 100; valores inválidos retornam 400. `parentId` e `rootsOnly=true` não podem ser combinados.

## Exclusão

Toda exclusão de categoria é lógica, inclusive quando ela não tem relações.

- Uma subcategoria não excluída bloqueia a exclusão do pai, mesmo estando inativa. No lote, todos os descendentes não excluídos precisam integrar a exclusão.
- Produtos não excluídos, inclusive inativos, exigem `moveProductsToCategoryId`. O destino precisa ser visível, da mesma loja e estar fora do lote de exclusão. A transferência preserva os outros dados do produto e ocorre na mesma transação da exclusão.
- Serviços não excluídos bloqueiam a exclusão com `Category.HasServices`; mova ou exclua esses serviços antes.
- Produtos, serviços e subcategorias já excluídos logicamente não bloqueiam. As linhas antigas são preservadas para manter suas referências.
- Uma falha em qualquer categoria do lote impede a gravação do lote inteiro.

## Rotas

O backoffice usa o envelope `Result` e exige `categories.view` para leitura ou `categories.manage` para escrita, com acesso integral do dono principal.

| Método e rota | Uso |
|---|---|
| POST `/api/categories/add` e `/add-range` | Cadastro individual/em lote; contratos anteriores, com `slug` opcional |
| PUT `/api/categories/update` e `/update-range` | Atualização individual/em lote com validação da árvore resultante |
| GET `/api/categories/tree` | Árvore administrativa, incluindo inativas |
| GET `/api/categories/search?name=sapatos&parentId=1&isActive=true&page=1&pageSize=20` | Busca paginada |
| PUT `/api/categories/{id}/active` | Body `{ "isActive": false }` |
| PUT `/api/categories/reorder` | Body `{ "items": [{ "id": 1, "displayOrder": 2 }, { "id": 2, "displayOrder": 1 }] }` |
| DELETE `/api/categories/{id}?moveProductsToCategoryId=2` | Exclusão individual com transferência opcional |
| DELETE `/api/categories/delete-with-products` | Body `{ "ids": [1, 3], "moveProductsToCategoryId": 2 }`; destino opcional |
| DELETE `/api/categories/delete/{id}` e `/delete-range` | Rotas anteriores; exclusão protegida sem transferência |
| GET `/api/storefront/categories/tree` | Árvore pública visível, sem autenticação |
| GET `/api/storefront/categories/by-slug/{slug}` | Categoria pública visível pelo slug, sem autenticação; aceita inclusive o slug `tree` |
| GET `/api/storefront/products` | Produtos ativos em categorias visíveis, com paginação |

As rotas de consulta administrativa anteriores continuam disponíveis. Os comandos individuais também têm handlers registrados e reutilizam as regras do lote.

## Migration

`EnforceUniqueCategorySlug` cria somente o índice único `(enterprise_id, slug)` em `categoria`, filtrado por `is_deleted = FALSE`. Antes de aplicá-la em uma base existente, corrija eventuais slugs duplicados por loja; a migration não renomeia categorias automaticamente. Ela não foi aplicada aos bancos de desenvolvimento nesta implementação.
