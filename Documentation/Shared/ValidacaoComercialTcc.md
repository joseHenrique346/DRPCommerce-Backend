# Validação integrada e demonstração do TCC

Parte 7 da divisão aprovada: validar contratação, upgrade, downgrade, vencimento,
recuperação e isolamento; documentar contratos e entregar um roteiro reproduzível.
A Parte 6 está no commit local `0b02bb6`; a Parte 7, em `013fdbf`, sem push.
As correções posteriores de arquitetura permanecem sem commit.

## O que é executado

O script local `scripts/Test-CommercialFlow.ps1` compila a solução
e executa testes sobre PostgreSQL local descartável. O modo `Full` executa toda a suíte;
`Demo`, os três cenários `CommercialEndToEndTests` com a categoria `TccDemo`.
Esse auxiliar fica fora do Git e foi preservado na máquina de desenvolvimento. Um clone
novo contém os testes, mas não contém o script.

Nos cenários novos, as duas APIs usam suas composições reais de controllers, autenticação,
validação de sessão, resolução de domínio, permissões, Refit e persistência. TestServer
transporta as chamadas HTTP em memória; não substituímos os leitores de identidade/contrato
nem o middleware por autenticação fictícia. Os únicos provedores financeiros são doubles
de teste: não há requisição de pagamento ao Mercado Pago real.

O banco configurado fornece apenas acesso ao servidor local. As fixtures criam bancos
`drp_contract_checks_*` e `drp_drop_appearance_*`, aplicam migrations neles e os removem
ao encerrar. Somente esses prefixos próprios são removidos. São necessárias permissões
de criação/remoção de bancos. As APIs não escutam portas TCP e seus workers são acionados
explicitamente nos cenários, para tornar relógio e reconciliação reproduzíveis.

## Executar

Pré-requisitos: SDK .NET 9 ou superior, PowerShell e PostgreSQL local disponível. Para
SDK superior, o script usa `DOTNET_ROLL_FORWARD=Major` somente durante sua execução.
Dependências são restauradas pelo build; `-NoRestore` permite usar o cache já preparado.

```powershell
./scripts/Test-CommercialFlow.ps1 -Mode Full -UseLocalDevelopmentServer
./scripts/Test-CommercialFlow.ps1 -Mode Demo -UseLocalDevelopmentServer -NoRestore
```

O switch lê `appsettings.Development.json` privado do Store sem imprimir sua conexão.
Também é possível fornecer `DRP_CONTRACTS_TEST_POSTGRES` no ambiente e omitir o switch.
Fora do script, testes PostgreSQL podem ser ignorados se faltar essa variável; o script
recusa esse resultado e não anuncia sucesso parcial.

Em um clone sem o auxiliar local, configure `DRP_CONTRACTS_TEST_POSTGRES` por canal privado
e execute os testes diretamente. Confira no resultado que nenhum teste foi ignorado:

```powershell
dotnet test ./DROP-Ecommerce/tests/StoreCommerce.Contracts.Tests/StoreCommerce.Contracts.Tests.csproj
```

Arquivos por execução, em `artifacts/commercial-validation/<execução>/`:

| Arquivo | Evidência |
| --- | --- |
| `build.log` | Compilação da solução |
| `tests.log` | Resultado e diagnósticos do runner |
| `commercial.trx` | Casos, resultados e saída capturada de cada cenário |
| `report.json` | Modo, horário UTC, contadores e resultado global, sem connection string |
| `demo-steps.txt` | Passos comprovados pelos três cenários da apresentação |

O modo Full exige pelo menos 189 testes nesta revisão; Demo exige três. Todos precisam
passar, sem ignorados. Os artefatos ficam fora do Git. Não editar o relatório para ocultar
uma falha: abrir o diagnóstico, corrigir o problema e gerar outra execução.

## Roteiro de apresentação

### 1. Contratar e vender

Abra o resultado de `Subscription_then_drop_purchase_uses_both_http_apis_and_confirms_stock_once`.
Explique os passos registrados no console/TRX:

1. Uma loja Gratuita recebe recusa ao tentar cadastrar produto DROP.
2. Criar checkout da mensalidade ainda não concede direito.
3. Simulamos confirmação da fatura recorrente. A sessão anterior deixa de ser aceita;
   um token atualizado observa a liberação do módulo.
4. O dono cria principal/variação e evento pela API administrativa. Um comprador acessa
   o domínio da loja, entra na fila e recebe quinze minutos desde a admissão.
5. O comprador reserva duas unidades a R$ 20,00 e inicia checkout de R$ 40,00 para a loja.
   Repetir checkout usa a mesma operação, sem outra criação financeira.
6. A confirmação simulada é processada duas vezes. O saldo cai de cinco para três apenas
   uma vez, os snapshots são preservados e não aparece outro pedido no Store.
7. Outro comprador e outro lojista recebem recusa para esse pedido. O gestor da loja
   correta confirma entrega. Conta da mensalidade e conta da venda permanecem distintas.

Na reserva/checkout, o comprador não escolhe preço final, identidade interna do cliente,
recebedor ou status pago. O CPF é informado pelo comprador e validado por formato/checksum;
não há prova documental de titularidade nesta etapa. O valor usado no cenário é sintético.
Não abrir uma URL hospedada como se a resposta fictícia fosse um checkout externo real.

### 2. Downgrade e recuperação

Abra `Scheduled_downgrade_blocks_drop_at_boundary_and_upgrade_requires_explicit_reactivation`.

1. Loja DRPCommerce paga agenda Pro. Antes do vencimento, DROP continua disponível.
2. No vencimento, a renovação menor é confirmada: exposição DROP desaparece, mas produto
   e cinco unidades continuam no painel privado, bloqueados pelo contrato.
3. Um upgrade pago restaura o direito. O evento continua bloqueado até o dono consultar
   a prévia e confirmar a reativação, sem republicação automática.

### 3. Carência e indisponibilidade

Abra `Grace_expiration_propagates_to_drop_even_during_provider_outage_and_recovery_is_explicit`.

1. Uma renovação rejeitada mantém acesso no sexto dia de carência.
2. No sétimo dia, simulamos provedor indisponível. O retorno ao Gratuito ainda é aplicado
   localmente, e o storefront DROP deixa de expor o evento.
3. Ao recuperar o pagamento, o período original é restaurado, sem iniciar outros trinta
   dias e sem republicar o produto automaticamente.

Os testes anteriores complementam esses três percursos com variantes/CPF, concorrência,
última vaga, transferências/rollback, aprovação tardia, resposta perdida, equipe, domínio,
aparência, logos, assinaturas, webhook assinado e exclusão de conta com revisão pendente.

## Contratos para o frontend

- [Planos, ofertas, direitos, consumo, solicitação e cobrança](PlanosEContratos.md).
- [Catálogo, fila, reserva, checkout e pedidos DROP](../Drop/Application/FluxoComercial.md).
- [Identidade, sessão, troca de loja e cabeçalhos](../Store/Api/Authentication.md).
- [Aparência, edição, publicação e recursos](Appearance.md).
- [Integração interna Store/DROP](../Drop/Infrastructure/StoreIntegration.md).

Os caminhos são dos serviços. O Gateway acrescenta `/api/store` ou `/api/drop`; por exemplo,
`/api/drop/api/storefront/{storeSlug}/drop/events`. Tela/cliente deve apresentar `financialReviewCode`
como revisão pendente e impedir nova tentativa de pagamento naquele pedido. O retorno do
Mercado Pago não concede plano nem confirma compra; consultar os endpoints do backend.

## Aplicação em outro ambiente

Esta validação não migra bancos existentes e não configura contas externas. Antes de publicar:

1. Inventariar versão/IDs legados, produtos, equipe, domínios, reservas e intenções pendentes;
   salvar backup e escolher bancos distintos Store/DROP. Mapeamentos comerciais legados
   estão nos testes e na documentação de planos, preservando seus IDs.
2. Revisar migrations/SQL e publicar primeiro o Store atualizado, com rotas internas protegidas,
   depois o DROP consumidor. Conferir revisões/ack em `contract/drop-sync` e workers ativos.
3. Configurar chaves privadas, domínios, URLs HTTPS e contas de teste corretas. Não copiar
   tokens fictícios dos cenários. A chave HMAC do CPF deve permanecer estável durante eventos.
4. Homologar autorização/recorrência, checkout e webhooks do Mercado Pago com contas de teste,
   sem assumir ambiente pelo prefixo da credencial ou somente por `live_mode`.
5. Verificar rede, forwarding, chaves, logs e cadência de reconciliação antes do teste de carga.
   Instalar o backend não instala temas/renderers ou uma interface de contratação.

A entrega comprova o backend com provedores simulados. Frontend, homologação externa,
testes de carga/abuso distribuído, reposição avançada de inventário e resolução de revisões
financeiras continuam pendentes. Melhor Envio vem após estas sete partes; o frete atual é
zero e não representa cotação ou etiqueta. E-mails e a investigação de estorno permanecem
adiados conforme o recorte acordado.

## Resultado desta entrega

Em 21/11/2026, o runner foi exercitado nos dois modos: Demo aprovou os três cenários;
Full aprovou 174 testes, sem ignorados, com compilação da solução sem erros. Também foram
verificadas as recusas de servidor remoto e conexão ausente antes do build/acesso ao banco.
As migrations só foram aplicadas nos bancos descartáveis das fixtures. Os relatórios desta
execução ficam em `artifacts/commercial-validation/20261121-161615-c9a4be07/`, fora do Git.

A Parte 7 foi commitada localmente em `013fdbf`, sem push. Após a correção de arquitetura
das Partes 5 e 6, Full aprovou 181 testes, sem falhas ou ignorados, com build sem erros.
Os sete testes adicionais cobrem dependências entre camadas, entrada dos controllers pelo
MediatR, handlers únicos, campos JSON e inscrição idempotente/isolada pela API.
O relatório dessa execução está em `artifacts/commercial-validation/20261121-203545-27e96c79/`.
As correções posteriores ao commit da Parte 7 permanecem sem commit para revisão.

Após o esclarecimento do padrão pelo usuário, a regra dos casos de uso foi transferida
para os handlers, os onze Services de regra/orquestração foram removidos e os workers
passaram a acionar Commands/Queries. Validators passaram a verificar a estrutura de entrada.
Essa revisão aprovou 189 testes, zero falhas e zero ignorados, com build sem erros,
em `artifacts/commercial-validation/20261122-004222-dfff9f9b/`.
Os oito testes adicionais verificam a ausência dos Services e a validação de catálogo,
reserva, CPF, transferência, importação, dinheiro/moeda/UTC e modo de reativação.
A revisão continua sem commit; Mercado Pago real e bancos existentes não foram utilizados.
