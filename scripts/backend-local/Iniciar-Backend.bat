@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul
title DRPCommerce - backend local

rem Inicia Store (5001), Drop (5002) e Gateway (5000), cada um em sua janela.

set "PACOTE=%~dp0"
set "PACOTE=%PACOTE:~0,-1%"
set "CONFIG=%PACOTE%\config.env"

if not exist "%PACOTE%\app\store\StoreCommerce.Api.exe" goto :sem_publicacao
if not exist "%PACOTE%\app\drop\DropCommerce.Api.exe" goto :sem_publicacao
if not exist "%PACOTE%\app\gateway\Commerce.Gateway.exe" goto :sem_publicacao
if not exist "%CONFIG%" goto :sem_config

for /f "usebackq eol=# tokens=1,* delims==" %%A in ("%CONFIG%") do set "%%A=%%B"

if not defined PROTOCOLO set "PROTOCOLO=https"
if /i "%PROTOCOLO%"=="https" set "PROTOCOLO=https"
if /i "%PROTOCOLO%"=="http" set "PROTOCOLO=http"
if not "%PROTOCOLO%"=="https" if not "%PROTOCOLO%"=="http" goto :protocolo_invalido

if not defined ASPNETCORE_ENVIRONMENT set "ASPNETCORE_ENVIRONMENT=Development"

call :validar_conexao STORECOMMERCE_CONNECTION_STRING
if errorlevel 1 goto :falha

call :validar_conexao DROPCOMMERCE_CONNECTION_STRING
if errorlevel 1 goto :falha

powershell -NoProfile -ExecutionPolicy Bypass -Command "$p = Get-NetTCPConnection -State Listen -LocalPort 5000, 5001, 5002 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty LocalPort -Unique; if ($p) { Write-Host ('[ERRO] Porta(s) já em uso: ' + ($p -join ', ') + '. Rode Parar-Backend.bat ou feche o Visual Studio.'); exit 1 }; exit 0"
if errorlevel 1 goto :falha

if "%PROTOCOLO%"=="http" goto :iniciar

rem Cria e instala, se ainda não existir, o certificado de localhost usado pelos três serviços.
powershell -NoProfile -ExecutionPolicy Bypass -File "%PACOTE%\certificado-local.ps1"
if errorlevel 1 goto :falha

set "Kestrel__Certificates__Default__Subject=DRPCommerce Backend Local"
set "Kestrel__Certificates__Default__Store=My"
set "Kestrel__Certificates__Default__Location=CurrentUser"

:iniciar
set "URL_GATEWAY=%PROTOCOLO%://localhost:5000"
set "URL_STORE=%PROTOCOLO%://localhost:5001"
set "URL_DROP=%PROTOCOLO%://localhost:5002"

set "STORECOMMERCE_API_BASE_URL=%URL_STORE%"
set "ReverseProxy__Clusters__cluster_store__Destinations__destino_principal__Address=%URL_STORE%"
set "ReverseProxy__Clusters__cluster_drop__Destinations__destino_principal__Address=%URL_DROP%"

rem Se o servico encerrar com erro, a janela fica aberta para mostrar o motivo.
start "DRPCommerce Store - %URL_STORE%" /D "%PACOTE%\app\store" cmd /c ""%PACOTE%\app\store\StoreCommerce.Api.exe" --urls %URL_STORE% || pause"
start "DRPCommerce Drop - %URL_DROP%" /D "%PACOTE%\app\drop" cmd /c ""%PACOTE%\app\drop\DropCommerce.Api.exe" --urls %URL_DROP% || pause"
start "DRPCommerce Gateway - %URL_GATEWAY%" /D "%PACOTE%\app\gateway" cmd /c ""%PACOTE%\app\gateway\Commerce.Gateway.exe" --urls %URL_GATEWAY% || pause"

echo.
echo Serviços iniciados em janelas separadas (ambiente %ASPNETCORE_ENVIRONMENT%).
echo.
echo   Gateway (use no front): %URL_GATEWAY%
echo   Store:                  %URL_STORE%/swagger
echo   Drop:                   %URL_DROP%/swagger
echo.
echo   Rotas pelo Gateway: %URL_GATEWAY%/api/store/... e %URL_GATEWAY%/api/drop/...
echo.
echo Para parar, feche as janelas dos serviços ou rode Parar-Backend.bat.
echo.
pause
exit /b 0

:validar_conexao
setlocal EnableDelayedExpansion
if not defined %~1 (
    echo [ERRO] %~1 não está preenchida em config.env.
    exit /b 1
)
if not "!%~1:SUA_SENHA=!"=="!%~1!" (
    echo [ERRO] Troque SEU_USUARIO e SUA_SENHA em %~1 no config.env.
    exit /b 1
)
exit /b 0

:sem_publicacao
echo [ERRO] Pasta app incompleta. Gere o pacote com scripts\publicar-backend-local.bat.
goto :falha

:sem_config
echo [ERRO] config.env não encontrado. Copie config.env.example para config.env e preencha.
goto :falha

:protocolo_invalido
echo [ERRO] PROTOCOLO deve ser https ou http. Valor atual: %PROTOCOLO%
goto :falha

:falha
echo.
pause
exit /b 1
