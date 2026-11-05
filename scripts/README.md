# Scripts

## Backend local para o front

`publicar-backend-local.bat` publica Store, Drop e Gateway em `artifacts/backend-local/`. O pacote gerado roda em Windows x64 sem Visual Studio e sem .NET instalado.

```text
artifacts/backend-local/
├── app/                   executáveis publicados (store, drop, gateway)
├── config.env             connection strings e chaves, nunca versionado
├── config.env.example
├── certificado-local.ps1  cria e instala o certificado HTTPS de localhost
├── Iniciar-Backend.bat    sobe Gateway 5000, Store 5001 e Drop 5002
├── Parar-Backend.bat
├── LEIA-ME.md             instruções para quem vai rodar o pacote
└── versao.txt             data e commit da publicação
```

## O que fica no git

| Caminho | Versionado | Conteúdo |
| --- | --- | --- |
| `scripts/publicar-backend-local.bat` | Sim | Script de publicação. |
| `scripts/backend-local/` | Sim | Modelos copiados para o pacote a cada publicação. |
| `artifacts/` | Não | Executáveis (cerca de 340 MB) e `config.env` com credenciais. Ignorado pelo `.gitignore`. |

Não versione os executáveis nem o `config.env`. Cada máquina gera o próprio pacote a partir do código.

Os arquivos de `scripts/backend-local/` são modelos. Para alterar o `Iniciar-Backend.bat`, por exemplo, edite o de `scripts/backend-local/`; a cópia em `artifacts/backend-local/` é sobrescrita na próxima publicação. O `.gitattributes` da raiz mantém os `.bat` com CRLF, necessário para o `cmd.exe`.

Alterações comuns de código não exigem mudar os scripts. Eles só precisam de ajuste quando mudam portas, caminhos ou nomes dos `.csproj`, quando surge um serviço novo ou quando há uma configuração obrigatória nova.

## Atualizar e rodar a partir do git

Requer git e .NET 9 SDK ou superior, com acesso ao NuGet. Quem publica não precisa fazer nada além do push.

Primeira vez:

1. Clone o repositório.
2. Rode `scripts\publicar-backend-local.bat`. Ele cria `artifacts\backend-local\config.env` com `Security__SigningKey` e `Security__InternalServiceKey` aleatórias.
3. Abra esse `config.env` e troque `SEU_USUARIO` e `SUA_SENHA` nas connection strings do Store e do Drop. As credenciais são passadas por canal privado, nunca pelo git.
4. Rode `artifacts\backend-local\Iniciar-Backend.bat`.

A cada atualização:

1. Pare os serviços com `artifacts\backend-local\Parar-Backend.bat`.
2. `git pull`
3. `scripts\publicar-backend-local.bat`
4. `artifacts\backend-local\Iniciar-Backend.bat`

As publicações substituem `app/` e os modelos, mas preservam `config.env`. Se `config.env.example` ganhar uma variável nova, copie a linha para o `config.env`, que não é atualizado automaticamente.

## Enviar o pacote pronto

Para quem não tem git ou .NET SDK: rode `scripts\publicar-backend-local.bat`, preencha o `config.env` e envie a pasta `artifacts\backend-local` inteira, por exemplo em `.zip`, por canal privado, porque o `config.env` contém a senha do banco. Cada versão nova exige publicar e enviar de novo.

## Observações

- O script remove `appsettings.Development.json` do Store e do Drop publicados, para que credenciais locais de quem publica não acompanhem o pacote. A configuração vem somente de `config.env`.
- Ao iniciar, o Store também executa as tarefas automáticas de expiração de pedidos pendentes e de reservas de estoque, limpeza de tokens e manutenção de aparência no banco configurado. Com o banco de produção, publique a partir do mesmo commit que está em produção para não aplicar regras diferentes. `versao.txt` mostra o commit; o sufixo `-dirty` indica alterações não commitadas.
