@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul

rem Publica Store, Drop e Gateway em artifacts\backend-local.
rem O pacote gerado roda sem Visual Studio e sem .NET instalado (self-contained win-x64).

for %%I in ("%~dp0..") do set "RAIZ=%%~fI"
set "SOLUCAO=%RAIZ%\DROP-Ecommerce"
set "MODELOS=%~dp0backend-local"
set "SAIDA=%RAIZ%\artifacts\backend-local"
set "APPS=%SAIDA%\app"
set "RUNTIME=win-x64"

echo.
echo DRPCommerce - publicação do backend local
echo Saída: %SAIDA%
echo.

where dotnet >nul 2>&1
if errorlevel 1 goto :sem_dotnet

powershell -NoProfile -ExecutionPolicy Bypass -Command "$base = $env:APPS + '\'; $p = Get-Process | Where-Object { $_.Path -and $_.Path.StartsWith($base, [StringComparison]::OrdinalIgnoreCase) }; if ($p) { exit 1 }; exit 0"
if errorlevel 1 goto :em_execucao

if exist "%APPS%" rmdir /s /q "%APPS%"
if exist "%APPS%" goto :falha_limpeza

call :publicar "Store" "src\Store\StoreCommerce.Api\StoreCommerce.Api.csproj" "store"
if errorlevel 1 goto :falha

call :publicar "Drop" "src\Drop\DropCommerce.Api\DropCommerce.Api.csproj" "drop"
if errorlevel 1 goto :falha

call :publicar "Gateway" "src\Gateway\Commerce.Gateway\Commerce.Gateway.csproj" "gateway"
if errorlevel 1 goto :falha

rem Configurações locais de quem publica podem conter credenciais; o pacote usa config.env.
del /q "%APPS%\store\appsettings.Development.json" 2>nul
del /q "%APPS%\drop\appsettings.Development.json" 2>nul

copy /y "%MODELOS%\Iniciar-Backend.bat" "%SAIDA%\" >nul || goto :falha
copy /y "%MODELOS%\Parar-Backend.bat" "%SAIDA%\" >nul || goto :falha
copy /y "%MODELOS%\LEIA-ME.md" "%SAIDA%\" >nul || goto :falha
copy /y "%MODELOS%\config.env.example" "%SAIDA%\" >nul || goto :falha
copy /y "%MODELOS%\certificado-local.ps1" "%SAIDA%\" >nul || goto :falha

if exist "%SAIDA%\config.env" goto :config_preservado

set "MODELO_CONFIG=%MODELOS%\config.env.example"
set "DESTINO_CONFIG=%SAIDA%\config.env"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$t = [IO.File]::ReadAllText($env:MODELO_CONFIG); $r = [Security.Cryptography.RandomNumberGenerator]::Create(); foreach ($n in 'GERAR_SIGNING_KEY', 'GERAR_SERVICE_KEY') { $b = New-Object byte[] 32; $r.GetBytes($b); $t = $t.Replace('__' + $n + '__', -join ($b | ForEach-Object { $_.ToString('x2') })) }; [IO.File]::WriteAllText($env:DESTINO_CONFIG, $t)"
if errorlevel 1 goto :falha
echo config.env criado com chaves aleatórias. Preencha as connection strings antes de iniciar.
goto :versao

:config_preservado
echo config.env existente foi preservado.

:versao
set "COMMIT=desconhecido"
for /f "delims=" %%C in ('git -C "%RAIZ%" describe --always --dirty 2^>nul') do set "COMMIT=%%C"

> "%SAIDA%\versao.txt" (
    echo Publicado em: %DATE% %TIME%
    echo Commit: %COMMIT%
    echo Runtime: %RUNTIME% self-contained
)

echo.
echo Publicação concluída.
echo Pasta do pacote: %SAIDA%
echo Para rodar: abra Iniciar-Backend.bat nessa pasta.
echo.
pause
exit /b 0

:publicar
echo.
echo === Publicando %~1 ===
dotnet publish "%SOLUCAO%\%~2" -c Release -r %RUNTIME% --self-contained true -o "%APPS%\%~3" -p:InstallGitHooks=false --nologo
exit /b %errorlevel%

:sem_dotnet
echo [ERRO] .NET SDK não encontrado no PATH. Instale o .NET 9 SDK ou superior.
goto :falha

:em_execucao
echo [ERRO] Há serviços do pacote em execução. Rode "%SAIDA%\Parar-Backend.bat" e publique de novo.
goto :falha

:falha_limpeza
echo [ERRO] Não foi possível limpar "%APPS%". Feche programas que usam essa pasta.
goto :falha

:falha
echo.
echo [ERRO] A publicação não foi concluída.
echo.
pause
exit /b 1
