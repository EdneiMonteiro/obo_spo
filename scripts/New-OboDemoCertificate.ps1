# Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
# See LICENSE and DISCLAIMER.md in the project root for details.

[CmdletBinding()]
param(
    [string]$Subject = 'CN=OboWebDemo',
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\certs'),
    [string]$PfxPassword = 'ChangeThisPassword!',
    [int]$ValidYears = 2
)

$ErrorActionPreference = 'Stop'

$resolvedOutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

$certificate = New-SelfSignedCertificate `
    -Subject $Subject `
    -FriendlyName 'OboWebDemo' `
    -CertStoreLocation 'Cert:\CurrentUser\My' `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm sha256 `
    -KeyExportPolicy Exportable `
    -NotAfter (Get-Date).AddYears($ValidYears)

$securePassword = ConvertTo-SecureString -String $PfxPassword -AsPlainText -Force
$cerPath = Join-Path $resolvedOutputDirectory 'obo-demo.cer'
$pfxPath = Join-Path $resolvedOutputDirectory 'obo-demo.pfx'

Export-Certificate -Cert $certificate -FilePath $cerPath -Force | Out-Null
Export-PfxCertificate -Cert $certificate -FilePath $pfxPath -Password $securePassword -Force | Out-Null

Write-Host "Certificate created in store: Cert:\CurrentUser\My"
Write-Host "Thumbprint: $($certificate.Thumbprint)"
Write-Host "Public certificate for Entra upload: $cerPath"
Write-Host "PFX backup of the private key: $pfxPath"
Write-Host ""
Write-Host 'Use this value in your .env file:'
Write-Host "  AZURE_CERTIFICATE_THUMBPRINT=$($certificate.Thumbprint)"