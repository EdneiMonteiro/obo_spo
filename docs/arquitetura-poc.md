# Arquitetura da PoC OBO com Graph e SharePoint

## Objetivo

Esta PoC demonstra um fluxo On-Behalf-Of (OBO) em que um usuario entra em uma aplicacao web ASP.NET Core e a aplicacao, atuando como middle-tier confidencial, obtem um token delegado para o Microsoft Graph e lista documentos do SharePoint que esse usuario realmente pode acessar.

O foco da demonstracao e:

- autenticar o usuario no navegador
- usar certificado X.509 no app registration, sem client secret
- chamar Microsoft Graph com permissoes delegadas
- resolver automaticamente site, biblioteca e pasta a partir de uma URL do SharePoint
- mostrar o fluxo tecnico sem expor tokens

## Componentes

### 1. Navegador

O usuario acessa a aplicacao web e inicia o login pela rota de Identity UI.

### 2. Aplicacao web `src/OboWebDemo`

E o componente principal da PoC. Ele:

- autentica com OpenID Connect usando Microsoft.Identity.Web
- usa o app registration como confidential client
- carrega o certificado pelo thumbprint configurado
- executa a aquisicao de token para Graph em nome do usuario autenticado
- chama o Graph para resolver o destino SharePoint e listar documentos
- apresenta os resultados em Razor Pages

Arquivos mais relevantes:

- `src/OboWebDemo/Program.cs`
- `src/OboWebDemo/Services/OboSharePointService.cs`
- `src/OboWebDemo/Pages/Index.cshtml`
- `src/OboWebDemo/Pages/Documents.cshtml`

### 3. Microsoft Entra ID

Responsavel por:

- autenticar o usuario
- emitir o token de login para a aplicacao web
- validar o certificado do app registration
- emitir o token delegado para Microsoft Graph

### 4. Microsoft Graph

Recebe o token delegado do usuario e responde chamadas para:

- resolver o site SharePoint
- localizar a biblioteca correta
- listar os itens do drive ou da pasta alvo

### 5. SharePoint Online

E o repositorio final dos documentos. A PoC respeita as permissoes reais do usuario. Se o usuario nao tiver acesso ao site, biblioteca ou pasta, a chamada Graph falha mesmo com a configuracao correta do OBO.

## Fluxo logico

```text
Usuario/Navegador
    |
    | 1. GET /
    | 2. GET /MicrosoftIdentity/Account/SignIn
    v
OboWebDemo
    |
    | 3. Redireciona para Microsoft Entra ID
    v
Microsoft Entra ID
    |
    | 4. Usuario autentica
    | 5. Entra devolve o usuario para /signin-oidc
    v
OboWebDemo
    |
    | 6. GET /Documents
    | 7. GetAccessTokenForUserAsync(GraphScopes)
    v
Microsoft Entra ID
    |
    | 8. Emite token delegado para Graph em nome do usuario
    v
OboWebDemo
    |
    | 9. GET /sites/... /drives/... /children
    v
Microsoft Graph
    |
    | 10. Consulta SharePoint Online
    v
SharePoint Online
    |
    | 11. Retorna documentos visiveis ao usuario
    v
OboWebDemo
    |
    | 12. Renderiza site, biblioteca, usuario e lista de documentos
    v
Usuario/Navegador
```

## Fluxo implementado na aplicacao web

### Login

- A landing page esta em `GET /`.
- O botao `Entrar` envia o usuario para `GET /MicrosoftIdentity/Account/SignIn`.
- Depois do login, a sessao autenticada passa a existir no app web.

### Execucao da demonstracao

- A pagina `GET /Documents` e a rota funcional da PoC.
- Essa pagina chama `OboSharePointService.GetDocumentsAsync()`.
- O servico registra passos tecnicos em memoria para exibir a secao `Technical Details`.

### OBO executor

No contexto desta PoC, o "executor" do OBO e a propria chamada feita dentro do app web ao entrar em `/Documents`:

1. O usuario ja esta autenticado no app web.
2. O servico pede um token para Graph com `GetAccessTokenForUserAsync`.
3. Microsoft.Identity.Web usa o app registration confidencial com certificado.
4. O token delegado recebido representa o mesmo usuario autenticado.
5. O servico usa esse token para chamar Graph e listar os documentos.

## Resolucao do alvo SharePoint

A PoC nao assume que a URL fornecida aponta sempre para o site raiz. O servico tenta resolver automaticamente:

- o site SharePoint correto
- a biblioteca de documentos correta
- a pasta correta, quando houver

Exemplo de entrada:

- `https://seutenant.sharepoint.com/OBO%20Documents`
- `https://seutenant.sharepoint.com/sites/Financeiro/Documentos%20Compartilhados`
- `https://seutenant.sharepoint.com/sites/Financeiro/Documentos%20Compartilhados/Contratos`

## Configuracao e segredos

O repositorio foi sanitizado para publicacao. Os valores sensiveis nao devem ficar em `appsettings.json`.

### Fonte principal de configuracao local

- `.env`
- variaveis de ambiente do sistema
- `appsettings.json` apenas como estrutura vazia/default

### Variaveis principais

Arquivo base: `.env.example`

- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CERTIFICATE_THUMBPRINT`
- `SHAREPOINT_URL`

### Mapeamento no app web

O app web converte essas variaveis para as chaves esperadas pelo ASP.NET Core:

- `AZURE_TENANT_ID` -> `AzureAd__TenantId`
- `AZURE_CLIENT_ID` -> `AzureAd__ClientId`
- `AZURE_CERTIFICATE_THUMBPRINT` -> `AzureAd__ClientCertificates__0__CertificateThumbprint`
- `SHAREPOINT_URL` -> `OboDemo__SharePointUrl`

## Decisoes de seguranca

- O app usa certificado X.509 em vez de client secret.
- O certificado publico `.cer` vai para o app registration.
- A chave privada permanece local, no store do Windows ou em arquivo `.pfx` somente para uso controlado.
- A UI nao mostra tokens, apenas passos tecnicos do fluxo.
- O acesso ao SharePoint continua delegado ao usuario autenticado.

## Diferenca entre a PoC e um desenho mais produtivo

Para simplificar a demonstracao, a PoC pode funcionar com um unico app registration para login e para o middle-tier. Em ambiente mais proximo de producao, o ideal e separar:

- uma aplicacao cliente/web
- uma aplicacao middle-tier/API

Mesmo assim, os conceitos centrais da demonstracao permanecem os mesmos:

- login do usuario
- token delegado
- OBO para Graph
- autorizacao final respeitando permissoes do usuario no SharePoint

## Resultado esperado

Quando a configuracao esta correta, a tela `Documents` mostra:

- usuario autenticado
- site resolvido
- biblioteca resolvida
- pasta, quando aplicavel
- lista de documentos
- secao `Technical Details` com os passos do fluxo OBO