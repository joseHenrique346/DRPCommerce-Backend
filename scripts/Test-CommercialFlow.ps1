[CmdletBinding()]
param(
    [ValidateSet('Full', 'Demo')]
    [string]$Mode = 'Full',
    [switch]$UseLocalDevelopmentServer,
    [switch]$NoRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$previousConnection = $env:DRP_CONTRACTS_TEST_POSTGRES
$previousRollForward = $env:DOTNET_ROLL_FORWARD
$testConnection = $previousConnection

if ($UseLocalDevelopmentServer)
{
    $settingsPath = Join-Path $repositoryRoot 'DROP-Ecommerce/src/Store/StoreCommerce.Api/appsettings.Development.json'

    if (-not (Test-Path -LiteralPath $settingsPath))
    {
        throw 'Configuração local não encontrada. Configure DRP_CONTRACTS_TEST_POSTGRES por canal privado.'
    }

    $settings = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
    $testConnection = $settings.ConnectionStrings.DefaultConnection
}

if ([string]::IsNullOrWhiteSpace($testConnection))
{
    throw 'Configure DRP_CONTRACTS_TEST_POSTGRES ou use -UseLocalDevelopmentServer. Testes de banco não podem ser ignorados nesta validação.'
}

$databaseSettings = [System.Data.Common.DbConnectionStringBuilder]::new()
$databaseSettings.set_ConnectionString($testConnection)
$serverKeys = @($databaseSettings.Keys | Where-Object { $_ -in @('Host', 'Server') })

if ($serverKeys.Count -ne 1 -or $databaseSettings[$serverKeys[0]] -notin @('localhost', '127.0.0.1', '::1'))
{
    throw 'Esta validação aceita somente PostgreSQL local. As fixtures criam e removem bancos próprios; não migram o banco informado.'
}

$runKey = [DateTimeOffset]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
$resultsDirectory = Join-Path $repositoryRoot "artifacts/commercial-validation/$runKey"
[System.IO.Directory]::CreateDirectory($resultsDirectory) | Out-Null
$buildLog = Join-Path $resultsDirectory 'build.log'
$testLog = Join-Path $resultsDirectory 'tests.log'
$trxPath = Join-Path $resultsDirectory 'commercial.trx'
$reportPath = Join-Path $resultsDirectory 'report.json'
$stepsPath = Join-Path $resultsDirectory 'demo-steps.txt'
$report = [ordered]@{
    mode = $Mode
    startedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    completedAtUtc = $null
    status = 'failed'
    databasePolicy = 'disposable-local-databases-only'
    paymentProvider = 'simulated'
    total = 0
    passed = 0
    failed = 0
    skipped = 0
    error = $null
}

try
{
    $env:DRP_CONTRACTS_TEST_POSTGRES = $testConnection
    $env:DOTNET_ROLL_FORWARD = 'Major'
    $solution = Join-Path $repositoryRoot 'DROP-Ecommerce/DROP-Ecommerce.sln'
    $project = Join-Path $repositoryRoot 'DROP-Ecommerce/tests/StoreCommerce.Contracts.Tests/StoreCommerce.Contracts.Tests.csproj'
    $buildArguments = @('build', $solution, '-p:InstallGitHooks=false', '--verbosity', 'minimal')

    if ($NoRestore)
    {
        $buildArguments += '--no-restore'
    }

    Write-Output 'Compilando a solução; os serviços não serão publicados ou iniciados em portas locais.'
    & dotnet @buildArguments *> $buildLog

    if ($LASTEXITCODE -ne 0)
    {
        throw 'Falha na compilação. Consulte build.log no diretório de resultados.'
    }

    $testArguments = @('test', $project, '--no-build', '--no-restore', '-p:InstallGitHooks=false',
        '--results-directory', $resultsDirectory, '--logger', 'trx;LogFileName=commercial.trx',
        '--logger', 'console;verbosity=normal')

    if ($Mode -eq 'Demo')
    {
        $testArguments += @('--filter', 'Category=TccDemo')
    }

    Write-Output "Executando $Mode com PostgreSQL descartável e provedores fictícios."
    & dotnet @testArguments *> $testLog
    $testExit = $LASTEXITCODE

    if (-not (Test-Path -LiteralPath $trxPath))
    {
        throw 'O runner não gerou o relatório TRX. Consulte tests.log.'
    }

    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
    $counters = $trx.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")

    if ($null -eq $counters)
    {
        throw 'Relatório TRX sem contadores de execução.'
    }

    $report.total = [int]$counters.GetAttribute('total')
    $report.passed = [int]$counters.GetAttribute('passed')
    $report.failed = [int]$counters.GetAttribute('failed')
    $report.skipped = [int]$counters.GetAttribute('notExecuted')
    $minimum = if ($Mode -eq 'Demo') { 3 } else { 174 }

    if ($testExit -ne 0 -or $report.total -lt $minimum -or $report.passed -ne $report.total -or $report.skipped -ne 0)
    {
        throw 'Validação incompleta: existem falhas, testes ignorados ou menos cenários que o mínimo esperado. Consulte tests.log e commercial.trx.'
    }

    $steps = [System.Collections.Generic.List[string]]::new()

    foreach ($case in $trx.SelectNodes("//*[local-name()='UnitTestResult']"))
    {
        if ($case.GetAttribute('testName') -notmatch 'CommercialEndToEndTests')
        {
            continue
        }

        $steps.Add($case.GetAttribute('testName'))
        $standardOutput = $case.SelectSingleNode("./*[local-name()='Output']/*[local-name()='StdOut']")

        if ($null -ne $standardOutput)
        {
            $steps.Add($standardOutput.InnerText)
        }

        $steps.Add('')
    }

    [System.IO.File]::WriteAllLines($stepsPath, $steps, [System.Text.UTF8Encoding]::new($false))
    $report.status = 'passed'
    Write-Output "$($report.passed) testes aprovados; nenhum teste ignorado."

    if ($Mode -eq 'Demo')
    {
        Get-Content -LiteralPath $stepsPath
    }
}
catch
{
    $report.error = $_.Exception.Message
    throw
}
finally
{
    $report.completedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    $report | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $reportPath -Encoding utf8
    $env:DRP_CONTRACTS_TEST_POSTGRES = $previousConnection
    $env:DOTNET_ROLL_FORWARD = $previousRollForward
    Write-Output "Relatórios: $resultsDirectory"
}
