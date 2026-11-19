# Deploy Docker ARM64 na Oracle

Cada VM executa somente seu servico. Os Dockerfiles ficam nos projetos executaveis; cada pasta abaixo contem um `compose.yaml` independente e um `.env.example`. PostgreSQL e Redis nao sao criados por esses Composes.

| Pasta | VM | Porta publicada |
| --- | --- | --- |
| `store/` | Vitrine: `10.0.2.96` | `10.0.2.96:8080` -> container `8080` |
| `drop/` | Drop: `10.0.2.85` | `10.0.2.85:8080` -> container `8080` |
| `gateway/` | YARP: `10.0.0.7`, publico `147.15.67.122` | `10.0.0.7:443` -> container `8443` |

As VMs Ampere A1 usam ARM64 (`uname -m`: `aarch64`). Instalar Docker Engine e Docker Compose, com o daemon habilitado na inicializacao. O runtime .NET acompanha as imagens; o host nao precisa de SDK para executa-las.

## Build das imagens

Na raiz do repositorio, executar os comandos abaixo com Docker em Linux containers. Funcionam tambem no PowerShell com Docker Desktop e emulacao ARM64 habilitada:

```bash
docker buildx build --platform linux/arm64 --load -f DROP-Ecommerce/src/Store/StoreCommerce.Api/Dockerfile -t drpcommerce/store:oracle-arm64 .
docker buildx build --platform linux/arm64 --load -f DROP-Ecommerce/src/Drop/DropCommerce.Api/Dockerfile -t drpcommerce/drop:oracle-arm64 .
docker buildx build --platform linux/arm64 --load -f DROP-Ecommerce/src/Gateway/Commerce.Gateway/Dockerfile -t drpcommerce/gateway:oracle-arm64 .
```

O contexto deve ser a raiz do repositorio: as APIs dependem dos projetos irmaos e de Shared. O SDK compila na arquitetura nativa do builder e publica para ARM64 usando `TARGETARCH`. A `.dockerignore` exclui configuracoes locais, certificados e artefatos.

Conferir a arquitetura de cada imagem:

```bash
docker image inspect drpcommerce/store:oracle-arm64 --format '{{.Os}}/{{.Architecture}}'
docker image inspect drpcommerce/drop:oracle-arm64 --format '{{.Os}}/{{.Architecture}}'
docker image inspect drpcommerce/gateway:oracle-arm64 --format '{{.Os}}/{{.Architecture}}'
```

O resultado deve ser `linux/arm64`. Para novas releases, preferir tags versionadas e atualizar `STORE_IMAGE`, `DROP_IMAGE` e `GATEWAY_IMAGE` nos respectivos `.env`.

## Entregar as imagens

Usar um registry ou exportar os arquivos para enviar a CloudOps. Para exportar no Windows, criar as pastas de entrega no PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path artifacts/oracle-docker/store,artifacts/oracle-docker/drop,artifacts/oracle-docker/gateway
```

Exportar cada imagem com Docker:

```bash
docker image save -o artifacts/oracle-docker/store/image.tar drpcommerce/store:oracle-arm64
docker image save -o artifacts/oracle-docker/drop/image.tar drpcommerce/drop:oracle-arm64
docker image save -o artifacts/oracle-docker/gateway/image.tar drpcommerce/gateway:oracle-arm64
```

Copiar para cada pasta de entrega seu `compose.yaml` e `.env.example` de `deploy/oracle/`. Juntar este guia e, nas APIs, o SQL de migration gerado abaixo. As tres pastas podem ser enviadas em um unico ZIP; CloudOps deve copiar apenas a pasta do respectivo modulo para cada VM. Nao incluir `.env` reais nem certificados privados no ZIP ou no git. Imagens e pacotes gerados ficam em `artifacts/`, ignorado pelo git.

Na pasta do servico recebida pela VM:

```bash
docker load -i image.tar
cp .env.example .env
chmod 600 .env
nano .env
```

Executar `cp` somente na primeira instalacao. Nas atualizacoes, preservar o `.env` existente e conferir novas variaveis e a tag da imagem.

Com registry, publicar as imagens no registro escolhido e configurar a referencia completa no `.env`. Exemplo para Store:

```bash
docker tag drpcommerce/store:oracle-arm64 REGISTRO/PROJETO/store:VERSAO
docker push REGISTRO/PROJETO/store:VERSAO
```

Na VM, executar `docker compose pull` antes de iniciar. Publicar uma imagem nao atualiza um container em execucao; a etapa de deploy continua necessaria.

## Variaveis de ambiente

Os Composes carregam `.env` via `env_file`. Preencher os modelos antes de iniciar:

- Vitrine: `STORECOMMERCE_CONNECTION_STRING`, banco `storecommercedb` em `10.0.2.169:5433`.
- Drop: `DROPCOMMERCE_CONNECTION_STRING`, banco `dropcommercedb` em `10.0.2.169:5432`; `STORECOMMERCE_API_BASE_URL=http://10.0.2.96:8080/`, sem `/api/store`.
- Nas duas APIs: `Security__SigningKey`, `Security__InternalServiceKey`, emissor, dominios e origins do frontend. Gerar duas chaves independentes com `openssl rand -hex 32` executado duas vezes e compartilhar os mesmos valores entre as APIs. Os placeholders sao intencionalmente curtos para impedir inicializacao com chaves de exemplo.
- `Security__TrustedProxies__0`: origem privada efetiva do YARP, esperada `10.0.0.7`.
- Gateway: destinos privados, dominios permitidos e certificado HTTPS, conforme a secao seguinte.

Manter segredos entre aspas simples para preservar `$`; para uma aspa simples no valor, usar `\'`. Aplicar tambem o escaping de connection string Npgsql quando necessario e configurar TLS do PostgreSQL conforme a politica da equipe.

Dentro dos containers as APIs escutam `+:8080`. O IP `10.0.2.x` pertence ao host e fica no bind de porta do Compose. Containers de VMs diferentes usam os IPs privados das VMs, nao nomes de servico de uma rede Docker local.

`AllowedOrigins` recebe origins exatas, com protocolo/porta e sem caminho ou barra final. O Store resolve a loja pelo Host ativo/verificado no banco: configurar DNS, certificado e roteamento para preservar esse dominio. CORS e DNS nao cadastram nem verificam uma loja automaticamente.

## HTTPS no Gateway

O Gateway termina TLS diretamente no Kestrel, em `8443` no container e `443` na VM. Provisionar um PFX valido com chave privada e cadeia no caminho `TLS_CERTIFICATE_PATH`; preencher `Kestrel__Certificates__Default__Password` e apontar o DNS publico para `147.15.67.122`.

O usuario `app` do container tem UID 1654 e precisa ler o arquivo. Exemplo de permissoes:

```bash
sudo chown root:1654 /opt/drpcommerce/certs/gateway.pfx
sudo chmod 640 /opt/drpcommerce/certs/gateway.pfx
```

O certificado e montado somente para leitura; o Compose falha se o arquivo nao existir. O health check usa HTTP em `127.0.0.1:8080` dentro do container, sem publicar essa porta. A porta 80 nao e publicada e nao ha redirecionamento HTTP implicito.

Nao configurar `ASPNETCORE_HTTPS_PORT=443` nas APIs privadas: a chamada direta Drop -> Store usa HTTP na rede privada protegida. Se houver outro proxy HTTP antes do YARP, revisar o forwarding no Gateway; este modelo pressupoe HTTPS chegando diretamente ao YARP.

## Migrations

As APIs nao aplicam migrations automaticamente. Na maquina de backend, com .NET SDK, CLI `dotnet-ef` 9 e configuracao de design-time disponiveis, gerar SQL idempotente do mesmo commit das imagens:

```bash
dotnet ef migrations script --idempotent --project DROP-Ecommerce/StoreCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Store/StoreCommerce.Api --output artifacts/oracle-docker/store/migrations.sql
dotnet ef migrations script --idempotent --project DROP-Ecommerce/DropCommerce.Infrastructure.Data --startup-project DROP-Ecommerce/src/Drop/DropCommerce.Api --output artifacts/oracle-docker/drop/migrations.sql
```

Gerar SQL nao aplica alteracoes. Fazer backup e revisar os scripts antes de executa-los. Com `psql` instalado na VM de cada API, aplicar o arquivo recebido na pasta do servico:

```bash
# Na Vitrine
psql -h 10.0.2.169 -p 5433 -U USUARIO_MIGRATION_STORE -d storecommercedb -W --set ON_ERROR_STOP=on --file migrations.sql

# No Drop
psql -h 10.0.2.169 -p 5432 -U USUARIO_MIGRATION_DROP -d dropcommercedb -W --set ON_ERROR_STOP=on --file migrations.sql
```

Bases legadas com historico divergente exigem reconciliacao antes do script idempotente. Voltar uma imagem nao desfaz migrations.

## Iniciar e atualizar

Na pasta de cada servico, depois de configurar o ambiente e preparar o banco:

```bash
docker compose config --quiet
docker compose up -d --pull never
docker compose ps
docker compose logs --tail 100
```

Iniciar Vitrine, depois Drop e depois Gateway. Para atualizar, carregar ou baixar a nova imagem, ajustar sua referencia no `.env`, aplicar migrations quando necessario e executar `docker compose up -d --pull never`.

Os containers usam usuario sem privilegios, ambiente `Production`, logs com rotacao, health check e `restart: unless-stopped`. Cada Compose tem um volume persistente de Data Protection; preservar os volumes nas atualizacoes. Nao usar `docker compose down -v` como rotina de deploy.

O `/health` retorna `200 Healthy` sem autenticacao e verifica somente o processo HTTP. Pelo Gateway:

```text
https://DOMINIO_API/health
https://DOMINIO_API/api/store/health
https://DOMINIO_API/api/drop/health
```

Testar tambem login/CSRF e uma operacao do Drop que consulte o Store. O Gateway bloqueia `/api/store/api/internal` e `/api/drop/api/internal` e seus descendentes; a integracao direta privada permanece disponivel. Swagger/OpenAPI das APIs so sao expostos em `Development`.

Um estado `unhealthy` sinaliza falha; o Docker nao reinicia o processo somente por esse estado. A politica de reinicio atua quando o processo termina e nas demais condicoes dessa politica.

## Rede para CloudOps

| Origem | Destino | TCP |
| --- | --- | --- |
| Internet | YARP (`147.15.67.122`) | 443 |
| YARP (`10.0.0.7`) | Vitrine (`10.0.2.96`) | 8080 |
| YARP (`10.0.0.7`) | Drop (`10.0.2.85`) | 8080 |
| Drop | Vitrine (`10.0.2.96`) | 8080 |
| Vitrine | PostgreSQL (`10.0.2.169`) | 5433 |
| Drop | PostgreSQL (`10.0.2.169`) | 5432 |
| IPs administrativos -> jump host | YARP -> VMs privadas | 22 |

Confirmar as origens com `ip route get`, considerando as duas VNICs e o NAT do Docker. Ajustar regras OCI, firewall Linux e `pg_hba.conf`. Portas publicadas pelo Docker passam por regras de encaminhamento/NAT; nao pressupor que regras `INPUT`/UFW existentes filtrem esse trafego. Conferir o backend de firewall e as regras apropriadas, como `DOCKER-USER` com iptables.

Redis nao e usado pelo codigo atual. As portas locais 5000/5001/5002 e a porta 5434 do antigo Zabbix nao participam deste deploy.

## Runtime

Os Dockerfiles acompanham o target atual `net9.0`. O .NET 9 encerrou suporte em 10/11/2026; planejar a migracao do projeto e das imagens para .NET 10 LTS para manter suporte em producao. A containerizacao nao atualiza automaticamente o target.

Referencias: [builds multiplataforma](https://docs.docker.com/build/building/multi-platform/), [compilacao cruzada .NET](https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/), [firewall Docker](https://docs.docker.com/engine/network/packet-filtering-firewalls/) e [suporte .NET](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
