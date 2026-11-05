# Backend local DRPCommerce

Pacote para rodar Store, Drop e Gateway sem Visual Studio. Os executáveis são self-contained para Windows x64, então não é preciso instalar o .NET para rodar.

## Antes da primeira execução

1. Se recebeu o pacote em `.zip`, clique com o botão direito no arquivo, abra Propriedades, marque "Desbloquear" e clique em OK antes de extrair. Extraia para uma pasta fixa; não rode os arquivos de dentro do `.zip`.
2. O banco PostgreSQL configurado precisa estar acessível e com as migrations aplicadas. As APIs não aplicam migrations sozinhas.
3. Abra `config.env` no Bloco de Notas. As connection strings do Store e do Drop não podem conter `SEU_USUARIO` nem `SUA_SENHA`. Esse arquivo contém a senha do banco: não o compartilhe nem o envie para o git.
4. Se o front não roda em `http://localhost:5173`, ajuste `Security__AllowedOrigins__0`.
5. Na primeira execução, o Windows pergunta se deseja instalar o certificado "DRPCommerce Backend Local". Clique em Sim. Ele permite que o navegador e os serviços confiem no HTTPS de `localhost` e vale só para o seu usuário do Windows. Se preferir não instalar, use `PROTOCOLO=http`; nesse modo o login com cookies não funciona.

Se `config.env` não existir, copie `config.env.example` para `config.env` e gere chaves com pelo menos 32 caracteres para `Security__SigningKey` e `Security__InternalServiceKey`.

## Rodar

Dê dois cliques em `Iniciar-Backend.bat`. Cada serviço abre em uma janela com seus logs; deixe as três abertas enquanto usar.

| Serviço | Endereço |
| --- | --- |
| Gateway (use no front) | https://localhost:5000 |
| Store | https://localhost:5001/swagger |
| Drop | https://localhost:5002/swagger |

No front, use `https://localhost:5000/api/store` e `https://localhost:5000/api/drop` como endereços da API. Exemplo: `https://localhost:5000/api/store/api/auth/csrf`.

## Parar

Feche as janelas dos serviços ou rode `Parar-Backend.bat`.

## Atualizar para uma versão nova

1. Rode `Parar-Backend.bat`.
2. Substitua os arquivos pela versão nova. Se a versão nova não trouxer `config.env`, mantenha o seu.
3. Se `config.env.example` tiver uma variável que não existe no seu `config.env`, copie a linha.
4. Rode `Iniciar-Backend.bat`. O certificado já instalado continua valendo.

`versao.txt` mostra a data e o commit usados para gerar este pacote.

## Problemas comuns

- Porta em uso: outra instância está rodando (Visual Studio ou este pacote). Rode `Parar-Backend.bat` ou feche o Visual Studio.
- Serviço encerrou com erro: a janela dele fica aberta mostrando o motivo. As causas mais comuns são connection string incorreta, banco fora do ar ou sem migrations e chave com menos de 32 caracteres.
- Clicou em Não no certificado: rode `Iniciar-Backend.bat` de novo e clique em Sim.
- "O Windows protegeu o computador": clique em "Mais informações" e em "Executar assim mesmo", ou desbloqueie o `.zip` antes de extrair.

Para remover o certificado, abra `certmgr.msc` e apague "DRPCommerce Backend Local" em Pessoal > Certificados e em Autoridades de Certificação Raiz Confiáveis > Certificados.
