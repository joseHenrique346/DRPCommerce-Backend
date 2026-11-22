# Padrão de leitura do projeto

Siga este padrão nas próximas alterações em C#, conforme solicitado pelo usuário:

- Escreva o corpo de `if`, `else` e laços na linha seguinte, com indentação de quatro espaços. Não use `if (...) return;`, `if (...) throw ...;` ou outros controles com o corpo na mesma linha.
- Em novos `if` com uma única instrução, prefira não usar chaves. Quando houver várias instruções, mantenha as chaves em linhas próprias.
- Separe validações e blocos de fluxo com uma linha em branco, assim como os grupos de instruções com finalidades diferentes.
- Escreva apenas uma instrução por linha. Não junte declarações, atribuições, chamadas, `await`, `return` ou `throw` na mesma linha.
- Coloque atributos de métodos em linhas próprias e não compacte métodos, construtores, `try/catch` ou blocos de lambdas.
- Preserve quebras de linha que ajudam na leitura de filtros, consultas e chamadas longas.
- Nos filtros dos DbContexts, use o nome da entidade em PascalCase para o parâmetro da lambda, conforme o padrão solicitado.

Quando o pedido for apenas organização, preserve condições, operadores, ordem de execução, contratos e comportamento. Não aproveite a formatação para refatorar a lógica.

# Organização por contexto

- Use nomes em inglês para as pastas e agrupe funcionalidades relacionadas por contexto, como `Catalog`, `Orders`, `Identity`, `StoreManagement`, `Commercial`, `Billing`, `Payments` e `Inventory`.
- Em Application, mantenha as features em `Features/Commands/<Context>/<Feature>`. Quando o contexto já identificar a feature, não repita o nome da pasta.
- Cada operação deve ter seus próprios arquivos de Command/Query, Handler e Validator. Coloque consultas em `Queries/<Operation>` e comandos em `<Operation>` dentro da feature.
- Mantenha contratos, auxiliares, validações, escopos e Services existentes dentro da feature correspondente, em subpastas com responsabilidade explícita. Não concentre operações diferentes em arquivos como `Commands.cs`, `Handlers.cs` ou `Validators.cs`.
- Separe interfaces em `Interfaces/<Context>`, com um tipo por arquivo. DTOs e implementações não devem ficar no arquivo de uma interface.
- Separe enums em `Enums/<Context>` na camada Application ou Domain correspondente, com um enum por arquivo. Não declare enums junto de entidades, Services ou DTOs.
- Integrações podem ficar em `Integrations/<Provider>`, separando interfaces, contratos e implementações técnicas. Os casos de uso continuam em `Features/Commands`.
- Tipos comuns utilizados por Store e DROP devem ficar no projeto da camada correspondente em `Shared`. Não duplique contratos ou utilitários idênticos entre os módulos.
- Aplique os mesmos contextos às pastas de entidades, configurações, repositórios e controllers. Preserve migrations geradas e os contratos de persistência ao reorganizar arquivos.
- Consulte `Documentation/Shared/OrganizacaoDoProjeto.md` para exemplos da estrutura.
