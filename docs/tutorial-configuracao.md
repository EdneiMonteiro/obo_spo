# Tutorial de configuracao da solucao

## Objetivo

Este tutorial mostra como configurar a PoC para demonstrar:

- login no navegador com Microsoft Entra ID
- aquisicao de token delegado para Microsoft Graph
- uso de certificado digital no app registration
- listagem de documentos de uma biblioteca SharePoint

O caminho da demo e a aplicacao web `src/OboWebDemo`.

Se voce quer repetir a demonstracao que acontecia em `localhost`, use a web app. O login acontece no navegador.

## Pre-requisitos

Voce precisa ter:

- acesso para criar ou administrar app registrations no tenant
- permissao para conceder admin consent, ou apoio de um administrador
- acesso de leitura ao site/biblioteca SharePoint alvo com o mesmo usuario que fara o login
- .NET 8 SDK instalado
- PowerShell no Windows

## Passo 1. Gerar o certificado da PoC

Use o script do repositorio para criar um certificado de laboratorio:

```powershell
.\scripts\New-OboDemoCertificate.ps1 -PfxPassword "ChangeThisPassword!"
```

Esse script entrega:

- um certificado com chave privada no store `CurrentUser\My`
- o arquivo publico `certs\obo-demo.cer`
- o arquivo privado `certs\obo-demo.pfx`
- o thumbprint do certificado

Use o `.cer` para upload no app registration. O thumbprint sera usado no `.env` da aplicacao web.

## Passo 2. Criar o app registration

No Microsoft Entra admin center:

1. Entre em `App registrations`.
2. Clique em `New registration`.
3. Defina um nome, por exemplo `OBO SharePoint Demo`.
4. Escolha `Accounts in this organizational directory only` para uma demo single-tenant, se isso atender ao seu cenario.
5. Em `Redirect URI`, escolha a plataforma `Web` e informe:

```text
https://localhost:7011/signin-oidc
```

6. Crie o registro.

Guarde estes valores:

- `Application (client) ID`
- `Directory (tenant) ID`

## Passo 3. Subir o certificado publico

No app registration:

1. Abra `Certificates & secrets`.
2. Va para `Certificates`.
3. Clique em `Upload certificate`.
4. Selecione o arquivo `certs\obo-demo.cer`.

Resultado esperado:

- o app registration passa a confiar no certificado publico
- a chave privada continua apenas no seu ambiente local

## Passo 4. Configurar autenticacao

No menu `Authentication` do app registration:

1. Confirme que a plataforma `Web` contem a redirect URI:

```text
https://localhost:7011/signin-oidc
```

Para a demo web, a parte critica e a redirect URI do app web.

## Passo 5. Configurar permissoes de API

No menu `API permissions`:

1. Clique em `Add a permission`.
2. Escolha `Microsoft Graph`.
3. Escolha `Delegated permissions`.
4. Adicione:

- `Sites.Read.All`
- `Files.Read.All`

5. Clique em `Grant admin consent` se o tenant exigir consentimento administrativo.

Observacao importante:

- a PoC usa permissoes delegadas, nao application permissions
- o usuario autenticado ainda precisa ter acesso real aos documentos no SharePoint

## Passo 6. Preparar o ambiente local

Na raiz do repositorio, crie o arquivo `.env` a partir do modelo:

```powershell
Copy-Item .env.example .env
```

Preencha o `.env` com os valores reais:

```env
AZURE_TENANT_ID=<tenant-id>
AZURE_CLIENT_ID=<client-id>
AZURE_CERTIFICATE_THUMBPRINT=<thumbprint-do-certificado>
SHAREPOINT_URL=https://seutenant.sharepoint.com/NomeDaBiblioteca
```

### O que cada valor representa

- `AZURE_TENANT_ID`: tenant do Microsoft Entra ID
- `AZURE_CLIENT_ID`: client id do app registration criado
- `AZURE_CERTIFICATE_THUMBPRINT`: thumbprint do certificado com chave privada instalado localmente
- `SHAREPOINT_URL`: URL do site, biblioteca ou pasta que a demo deve resolver

## Passo 7. Conferir os arquivos de configuracao do repositorio

Os arquivos principais ja estao preparados para ler o `.env`:

- `src/OboWebDemo/appsettings.json`
- `src/OboWebDemo/Program.cs`

No app web, o mapeamento efetivo e:

- `AZURE_TENANT_ID` -> `AzureAd:TenantId`
- `AZURE_CLIENT_ID` -> `AzureAd:ClientId`
- `AZURE_CERTIFICATE_THUMBPRINT` -> `AzureAd:ClientCertificates[0]:CertificateThumbprint`
- `SHAREPOINT_URL` -> `OboDemo:SharePointUrl`

## Passo 8. Rodar a aplicacao web

Na raiz do repositorio:

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project .\src\OboWebDemo\OboWebDemo.csproj --launch-profile https
```

Se voce quiser usar o comando `dotnet` sem caminho completo, primeiro ajuste a sessao atual do PowerShell:

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:USERPROFILE\.dotnet;$env:PATH"
dotnet run --project .\src\OboWebDemo\OboWebDemo.csproj --launch-profile https
```

Abra no navegador:

```text
https://localhost:7011
```

## Passo 9. Executar a chamada de login

Voce pode testar o login de duas formas:

### Opcao A. Pela UI

1. Abra `https://localhost:7011`.
2. Clique no botao `Entrar`.

### Opcao B. Pela rota direta

Abra diretamente:

```text
https://localhost:7011/MicrosoftIdentity/Account/SignIn
```

Resultado esperado:

- o navegador redireciona para Microsoft Entra ID
- o usuario autentica
- o retorno acontece pela redirect URI `/signin-oidc`
- a aplicacao passa a mostrar o nome do usuario autenticado

## Passo 10. Executar o OBO executor da demo

Na implementacao atual, o executor da demo e a pagina protegida `Documents`:

```text
https://localhost:7011/Documents
```

Ao acessar essa rota:

1. A pagina chama `OboSharePointService.GetDocumentsAsync()`.
2. O servico usa `GetAccessTokenForUserAsync(OboDemo:GraphScopes)`.
3. O Microsoft.Identity.Web adquire um token delegado para Graph usando o app registration com certificado.
4. O servico resolve a URL do SharePoint configurada.
5. O servico chama Graph para listar os itens da biblioteca ou pasta alvo.
6. A UI mostra o usuario autenticado, o site resolvido, a biblioteca e os documentos.
7. A area `Technical Details` mostra os passos sem expor tokens.

## Passo 11. Validar o resultado esperado

Se tudo estiver correto, a pagina `Documents` mostra:

- `Usuario autenticado`
- `Site resolvido`
- `Biblioteca`
- lista de documentos retornados pelo Graph
- `Technical Details` com o fluxo tecnico

## Problemas comuns

### Login falha ou redireciona com erro

Verifique:

- se a redirect URI `https://localhost:7011/signin-oidc` existe no app registration
- se o `AZURE_CLIENT_ID` aponta para o app registration correto
- se o `AZURE_TENANT_ID` esta correto

### OBO falha ao pedir token para Graph

Verifique:

- se o certificado `.cer` foi enviado ao app registration
- se o certificado com chave privada existe localmente no store `CurrentUser\My`
- se o thumbprint em `.env` corresponde ao certificado instalado
- se `Sites.Read.All` e `Files.Read.All` foram adicionados como delegated permissions
- se o admin consent foi concedido quando necessario

### A aplicacao autentica, mas nao lista documentos

Verifique:

- se `SHAREPOINT_URL` aponta para uma URL valida
- se o usuario autenticado tem acesso real ao SharePoint
- se a biblioteca ou pasta existem

## Resumo rapido do fluxo

1. O usuario entra pela UI web.
2. A aplicacao recebe a sessao autenticada.
3. A rota `/Documents` dispara a aquisicao do token delegado para Graph.
4. O Graph consulta o SharePoint com identidade do usuario.
5. A UI mostra os documentos sem expor credenciais nem tokens.