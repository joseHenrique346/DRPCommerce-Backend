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
- Mantenha contratos, auxiliares, validações e escopos dentro da feature correspondente, em subpastas com responsabilidade explícita. Não concentre operações diferentes em arquivos como `Commands.cs`, `Handlers.cs` ou `Validators.cs`.
- Use Auxiliar para as pastas de estruturas e auxiliares internos. Esse nome em português é uma exceção solicitada pelo usuário à regra de nomes de pastas em inglês.
- Não use uma pasta genérica Models nas features. Coloque entradas e seus componentes em Contracts/Requests, saídas e seus componentes em Contracts/Responses e estruturas exclusivamente internas em Auxiliar.
- Requests e Responses também incluem contratos de Commands, Queries e integrações internas. Contextos, seleções e estados de execução não devem ser classificados como contratos só por transportarem dados entre auxiliares. Reutilize componentes compartilhados sem duplicar tipos.
- Separe interfaces em `Interfaces/<Context>`, com um tipo por arquivo. DTOs e implementações não devem ficar no arquivo de uma interface.
- Separe enums em `Enums/<Context>` na camada Application ou Domain correspondente, com um enum por arquivo. Não declare enums junto de entidades, Services ou DTOs.
- Integrações podem ficar em `Integrations/<Provider>`, separando interfaces, contratos e implementações técnicas. Os casos de uso continuam em `Features/Commands`.
- Tipos comuns utilizados por Store e DROP devem ficar no projeto da camada correspondente em `Shared`. Não duplique contratos ou utilitários idênticos entre os módulos.
- Aplique os mesmos contextos às pastas de entidades, configurações, repositórios e controllers. Preserve migrations geradas e os contratos de persistência ao reorganizar arquivos.
- Consulte `Documentation/Shared/OrganizacaoDoProjeto.md` para exemplos da estrutura.

# Responsabilidades dos casos de uso

- Cada Handler representa um caso de uso com um objetivo principal identificável pelo nome. As decisões e a sequência de execução desse caso de uso devem ficar compreensíveis no próprio Handler.
- Cada Handler concreto atende um único contrato de request. Separe prévia de confirmação e operações distintas que tenham rotas próprias, preservando seus contratos HTTP.
- Operações CRUD singulares não devem encaminhar toda a execução para o Handler de lote. Preserve os bases existentes dos lotes e mantenha a operação singular explícita no seu Handler.
- Um caso de uso pode exigir várias verificações e efeitos relacionados ao mesmo objetivo. Não crie um Handler para cada condição ou operação interna.
- Commands e Queries representam os contratos; Validators verificam formato e estrutura de entrada; Handlers coordenam regras, repositórios, adapters e gravações.
- Não concentre casos de uso completos em Services por contexto e não crie um Service para cada Handler. Não renomeie um Service de negócio para Writer, Manager, Processor ou outro nome mantendo a mesma concentração de operações.
- Writers podem executar fragmentos específicos e efetivamente compartilhados de uma alteração. O Handler continua decidindo a operação e coordenando sua transação; o Writer não recebe o caso de uso inteiro nem confirma sozinho a transação do caso de uso.
- Auxiliares de consulta, seleção, projeção e políticas devem ter nomes e responsabilidades específicos. Preserve invariantes das entidades e políticas puras de domínio, sem duplicá-las nos Handlers.
- Quantidade de chamadas, isoladamente, não justifica uma abstração. Distingua reutilização de um fragmento da delegação de um caso de uso completo.
- Reutilize os bases de CRUD e Identity existentes quando forem adequados. Não acrescente hooks ou novos bases para acomodar outra arquitetura.
- Preserve a ordem dos locks, os limites transacionais, os escopos de gravação, a idempotência, a recuperação de resultados desconhecidos e os contratos públicos durante a transferência de regras.
- Composições pelo MediatR devem nomear etapas independentes necessárias ao mesmo objetivo, como expiração antes da reserva e reconciliação após o commit. Não use outro Handler como substituto da implementação principal nem injete implementações concretas de Handlers.
- Controllers cuidam do protocolo HTTP e entram pelo MediatR; workers cuidam de agendamento e descoberta e acionam os casos de uso. SQL, leases e gravações atômicas ficam em repositórios, sem deslocar a decisão comercial do Handler.
- Integrações técnicas podem manter adapters de credenciais, criptografia e comunicação externa. Isso não autoriza concentrar decisões comerciais nesses adapters.
