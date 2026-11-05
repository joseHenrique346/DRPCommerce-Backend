@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul

rem Encerra apenas os processos executados a partir da pasta app deste pacote.

set "PACOTE=%~dp0"
set "PACOTE=%PACOTE:~0,-1%"

powershell -NoProfile -ExecutionPolicy Bypass -Command "$base = $env:PACOTE + '\app\'; $janelas = @(Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'cmd.exe' -and $_.CommandLine -and $_.CommandLine.IndexOf($base, [StringComparison]::OrdinalIgnoreCase) -ge 0 }); $p = @(Get-Process | Where-Object { $_.Path -and $_.Path.StartsWith($base, [StringComparison]::OrdinalIgnoreCase) }); $janelas | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }; if ($p.Count -gt 0) { $p | Stop-Process -Force -ErrorAction SilentlyContinue; Write-Host ('Serviços encerrados: ' + (($p | ForEach-Object { $_.ProcessName }) -join ', ')) } else { Write-Host 'Nenhum serviço deste pacote estava em execução.' }"

echo.
pause
