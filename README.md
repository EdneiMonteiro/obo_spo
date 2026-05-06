# OBO Graph SharePoint Demo

## Visão Geral

Este repositório contém código de exemplo / prova de conceito (PoC) com o objetivo de demonstrar como implementar o fluxo On-Behalf-Of (OBO) com Microsoft Graph para listar documentos do SharePoint em nome do usuário autenticado, utilizando .NET 8, Microsoft Entra ID e Microsoft Graph.

Este projeto foi criado para fins de aprendizado, avaliação e experimentação.

## Aviso Importante

Este repositório contém **código de exemplo e não é destinado para uso em produção**.

Antes de utilizar qualquer parte deste projeto em um ambiente produtivo ou crítico, é essencial revisar, validar, proteger e adaptar o código conforme os requisitos da sua organização, incluindo:

- Segurança
- Escalabilidade
- Confiabilidade
- Monitoramento
- Observabilidade
- Custos
- Conformidade

Leia também:

- [DISCLAIMER.md](./DISCLAIMER.md)

## O que este exemplo demonstra

- Fluxo On-Behalf-Of (OBO) com Microsoft Entra ID para aquisição de token delegado
- Listagem de documentos do SharePoint via Microsoft Graph em nome do usuário autenticado
- Resolução automática de site, biblioteca ou pasta a partir de uma URL do SharePoint
- Autenticação com certificado (Shared Key para PoC, Managed Identity recomendado para produção)

## Pré-requisitos

- .NET 8 SDK
- Uma App Registration no Microsoft Entra ID com permissões delegadas (`Sites.Read.All`, `Files.Read.All`)
- Um certificado (self-signed para lab ou CA-issued para produção)
- Acesso a um site/biblioteca do SharePoint

## Como iniciar

1. Clone este repositório
2. Gere o certificado de demo:
   ```powershell
   .\scripts\New-OboDemoCertificate.ps1 -PfxPassword "ChangeThisPassword!"
   ```
3. Configure o `.env` na raiz do repositório:
   - `AZURE_TENANT_ID`: GUID do tenant
   - `AZURE_CLIENT_ID`: Client ID da app registration
   - `AZURE_CERTIFICATE_THUMBPRINT`: Thumbprint do certificado
   - `SHAREPOINT_URL`: URL do site/biblioteca/pasta do SharePoint
4. Execute em ambiente não produtivo:
   ```powershell
   dotnet run --project .\src\OboWebDemo\OboWebDemo.csproj --launch-profile https
   ```
5. Acesse `https://localhost:7011` e valide o comportamento antes de qualquer adaptação

## Limites da demo

- OBO de verdade pressupõe que o access token do usuário chega a um middle-tier separado. Aqui a demonstração foi simplificada para uma única web app confidencial.
- O fluxo usa permissão delegada — só lista documentos que o usuário autenticado realmente pode acessar.
- Se o tenant bloquear consentimento do usuário, o admin precisa conceder consentimento para `Sites.Read.All` e `Files.Read.All`.
- O certificado local precisa conter chave privada. O upload no Entra ID usa apenas o `.cer` público.

## Aviso Legal

O uso deste projeto está sujeito aos termos descritos em [DISCLAIMER.md](./DISCLAIMER.md).

## Contribuições

Contribuições podem ser aceitas a critério do mantenedor.

## Marcas Registradas (Trademarks)

Os nomes e serviços da Microsoft são utilizados apenas para fins descritivos.

Este projeto **não é afiliado, endossado ou suportado oficialmente pela Microsoft**.

O uso de marcas da Microsoft não deve sugerir qualquer tipo de parceria ou suporte oficial.