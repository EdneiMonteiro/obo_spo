<#
This Sample Code is provided for the purpose of illustration only and is not
intended to be used in a production environment. THIS SAMPLE CODE AND ANY
RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

We grant You a nonexclusive, royalty-free right to use and modify the Sample
Code and to reproduce and distribute the object code form of the Sample Code,
provided that You agree: (i) to not use Our name, logo, or trademarks to market
Your software product in which the Sample Code is embedded; (ii) to include a
valid copyright notice on Your software product in which the Sample Code is
embedded; and (iii) to indemnify, hold harmless, and defend Us and Our
suppliers from and against any claims or lawsuits, including attorneys' fees,
that arise or result from the use or distribution of the Sample Code.

Please note: None of the conditions outlined in the disclaimer above will
supersede the terms and conditions contained within the Customers Support
Services Description.
#>

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