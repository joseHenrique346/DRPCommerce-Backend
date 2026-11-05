# Garante um certificado HTTPS para localhost, confiável apenas para o usuário atual do Windows.
# Usado por Iniciar-Backend.bat; dispensa o .NET SDK e o "dotnet dev-certs".

$ErrorActionPreference = 'Stop'

$assunto = 'CN=DRPCommerce Backend Local'

$certificado = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq $assunto -and $_.HasPrivateKey -and $_.NotAfter -gt (Get-Date).AddDays(7) } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if (-not $certificado)
{
    Write-Host 'Criando certificado HTTPS para localhost...'

    $certificado = New-SelfSignedCertificate `
        -Subject $assunto `
        -FriendlyName 'DRPCommerce Backend Local' `
        -TextExtension @('2.5.29.17={text}DNS=localhost&IPAddress=127.0.0.1&IPAddress=::1', '2.5.29.37={text}1.3.6.1.5.5.7.3.1') `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -KeyExportPolicy NonExportable `
        -NotAfter (Get-Date).AddYears(2) `
        -CertStoreLocation Cert:\CurrentUser\My
}

$confiavel = Get-ChildItem Cert:\CurrentUser\Root | Where-Object { $_.Thumbprint -eq $certificado.Thumbprint }

if (-not $confiavel)
{
    Write-Host ''
    Write-Host 'O Windows vai perguntar se deseja instalar o certificado "DRPCommerce Backend Local".'
    Write-Host 'Clique em Sim. Isso só é pedido uma vez.'

    $raiz = New-Object Security.Cryptography.X509Certificates.X509Store('Root', 'CurrentUser')
    $raiz.Open('ReadWrite')

    try
    {
        $raiz.Add([Security.Cryptography.X509Certificates.X509Certificate2]::new($certificado.RawData))
    }
    catch
    {
        Write-Host '[ERRO] O certificado não foi marcado como confiável. Rode Iniciar-Backend.bat de novo e clique em Sim.'
        exit 1
    }
    finally
    {
        $raiz.Close()
    }
}

exit 0
