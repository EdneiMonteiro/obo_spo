# OBO Graph SharePoint Demo

## Disclaimer

This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment. THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

We grant You a nonexclusive, royalty-free right to use and modify the Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that You agree: (i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded; (ii) to include a valid copyright notice on Your software product in which the Sample Code is embedded; and (iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys' fees, that arise or result from the use or distribution of the Sample Code.

Please note: None of the conditions outlined in the disclaimer above will supersede the terms and conditions contained within the Customers Support Services Description.

## Sobre

Esta PoC demonstra como usar o fluxo On-Behalf-Of (OBO) com Microsoft Graph para listar documentos do SharePoint em nome do usuario autenticado. O fluxo OBO exige uma aplicacao confidencial (middle-tier) que troca o token do usuario por um token delegado para Graph.

## Caminho principal da PoC

O fluxo principal desta PoC e a aplicacao web em `src/OboWebDemo`.

Nesse fluxo:

- o usuario entra no navegador em `https://localhost:7011`
- o login acontece com redirecionamento normal para Microsoft Entra ID
- a pagina `Documents` executa a aquisicao do token delegado para Graph em nome do usuario autenticado
- os documentos do SharePoint sao renderizados no proprio app web

Se voce esta demonstrando a solucao para um cliente, esse e o caminho certo.

## O que a demo faz

1. O usuario acessa `https://localhost:7011`.
2. O app web redireciona o usuario para login no Microsoft Entra ID.
3. Depois do login, a pagina `Documents` adquire um token delegado para Graph em nome do usuario autenticado.
4. O servico resolve automaticamente se a URL informada aponta para site, biblioteca ou pasta.
5. O app chama Graph para listar os itens da biblioteca ou pasta resolvida.

## Setup no Microsoft Entra ID

Para a demo web mais simples, use uma unica app registration:

1. Crie uma app registration.
2. Em Authentication, configure a redirect URI web `https://localhost:7011/signin-oidc`.
3. Em Certificates & secrets > Certificates, faça upload do arquivo `.cer` do certificado publico.
4. Em API permissions:
   - adicione Microsoft Graph > Delegated permissions > `Sites.Read.All`
   - adicione Microsoft Graph > Delegated permissions > `Files.Read.All`
   - conceda admin consent se o tenant exigir

Essas permissoes sao delegadas. O token final continua representando o usuario autenticado, entao o usuario precisa ter acesso real ao conteudo no SharePoint.

## Gerar certificado de demo

Use [scripts/New-OboDemoCertificate.ps1](scripts/New-OboDemoCertificate.ps1) para gerar um certificado self-signed de laboratorio e exportar os arquivos necessarios:

```powershell
.\scripts\New-OboDemoCertificate.ps1 -PfxPassword "ChangeThisPassword!"
```

O script faz estas entregas:

- cria o certificado no store `CurrentUser\My`
- exporta `certs\obo-demo.cer` para upload no Entra ID
- exporta `certs\obo-demo.pfx` como backup opcional da chave privada
- imprime o thumbprint para preencher o `.env`

## Setup no SharePoint

Para o seu caso, a URL alvo esta configurada localmente via `.env` e consumida pela web app.

Confira estes pontos:

1. Abra a URL no navegador com o mesmo usuario que fara login na web app.
2. Confirme se a URL abre uma biblioteca ou pasta valida.
3. Se o usuario nao enxergar o conteudo, adicione-o ao site ou a propria biblioteca com permissao de leitura, por exemplo em Visitors ou Members.
4. Para esta demo delegada com OBO, nao e necessario configurar `Sites.Selected` nem permissoes de aplicativo no SharePoint.

## Configuracao

Preencha o arquivo `.env` na raiz do repositorio com:

- `AZURE_TENANT_ID`: GUID do tenant ou dominio, por exemplo `contoso.onmicrosoft.com`
- `AZURE_CLIENT_ID`: client id da app registration
- `AZURE_CERTIFICATE_THUMBPRINT`: thumbprint do certificado carregado no store local
- `SHAREPOINT_URL`: URL do site, da biblioteca ou de uma pasta do SharePoint

Para a web app, o caminho principal continua sendo `.env` + `https://localhost:7011`.

Para a sua URL atual, o codigo tenta resolver automaticamente:

- o site SharePoint correto
- a biblioteca `OBO Documents`, se a URL for de biblioteca
- a pasta, se a URL apontar para uma subpasta

## Executar a web app

Em uma nova janela do PowerShell, rode:

```powershell
& "$env:USERPROFILE\.dotnet\dotnet.exe" run --project .\src\OboWebDemo\OboWebDemo.csproj --launch-profile https
```

Depois abra:

```text
https://localhost:7011
```

Esse e o fluxo da PoC. O login acontece no navegador.

## Limites da demo

- OBO de verdade pressupoe que o access token do usuario chega a um middle-tier separado. Aqui a demonstracao foi simplificada para uma unica web app confidencial que adquire token delegado para Graph em nome do usuario autenticado.
- O fluxo usa permissao delegada. Ele so vai listar documentos que o usuario autenticado realmente pode acessar.
- Se o tenant bloquear consentimento do usuario, o admin precisa conceder consentimento para `Sites.Read.All` e `Files.Read.All`.
- Se a URL apontar para uma biblioteca sem acesso para o usuario, o Graph vai falhar mesmo com o OBO configurado corretamente.
- O certificado local precisa conter chave privada. O upload no Entra ID usa apenas o `.cer` publico.