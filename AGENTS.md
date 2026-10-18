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
