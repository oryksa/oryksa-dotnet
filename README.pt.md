<p align="center"><img src="https://app.oryksa.com/static/oryksa_logo.png" width="300" alt="ORYKSA"></p>

# Oryksa for .NET

[![English](https://img.shields.io/badge/English-30363d?style=for-the-badge)](README.md) [![Português](https://img.shields.io/badge/Portugu%C3%AAs-0891b2?style=for-the-badge)](README.pt.md) [![Español](https://img.shields.io/badge/Espa%C3%B1ol-30363d?style=for-the-badge)](README.es.md) [![Français](https://img.shields.io/badge/Fran%C3%A7ais-30363d?style=for-the-badge)](README.fr.md)

SDK oficial .NET da **ORYKSA AI Employees**, para apps desktop Windows e Linux (WPF, WinUI, Avalonia, MAUI), servidores (ASP.NET) e workers. O mesmo colaborador de IA que responde aos teus clientes no WhatsApp e no teu site, dentro da tua app.

* Documentação: https://developer.oryksa.com/pt/sdks
* API: `https://api.oryksa.com/v1`
* netstandard2.0 e net8.0, sem dependências no .NET 8.

## Instalar

```bash
dotnet add package Oryksa
```

## Como funciona (e porque é que a chave fica segura)

1. O teu **servidor** guarda a chave secreta da API (`oryk_live_...`, criada em developer.oryksa.com) e cria um **token de sessão** de curta duração para cada utilizador com sessão iniciada.
2. A tua **app** recebe só esse token de sessão (`oryk_cs_...`). A chave secreta nunca entra na app.
3. Cada resposta da IA conta como uma interação do teu plano ORYKSA, tal como no WhatsApp.

## Na app

```csharp
using Oryksa;

var client = new OryksaClient(getToken: async ct => await MyBackend.FetchOryksaTokenAsync(ct)); // "oryk_cs_..."
var agent = await client.AgentAsync();                 // nome, foto, saudação, sugestões
var reply = await client.SendAndWaitAsync("Are you open on Saturday?");
```

Constrói a tua janela de chat com `agent.Name`, `agent.Avatar` (o nome e a foto de "A tua IA" na ORYKSA) e `OryksaAgent.Pick(agent.Greeting, "en")`. Para teres um chat pronto com o aspeto da ORYKSA numa app desktop, aloja o chat do site numa WebView (WebView2 ou CefSharp) ou usa o SDK de JavaScript em Electron ou Tauri.

## No teu servidor (chave secreta)

```csharp
var oryksa = new OryksaServer(Environment.GetEnvironmentVariable("ORYKSA_API_KEY")!);
var session = await oryksa.CreateSessionAsync(conversationId: "user-42", customerName: "Ana");
// envia só session.GetProperty("client_token") para a app

await oryksa.SetPagesAsync(new[] { new Dictionary<string, string> { ["title"] = "Workouts", ["content"] = "Tap + to log a workout." } });
```

## Webhooks

```csharp
var evt = OryksaWebhook.Verify(rawBody, Request.Headers["ORYKSA-Signature"], webhookSecret);
```

## Erros

As chamadas falhadas lançam `OryksaException` com `Status`, `Code` (por exemplo `interaction_limit_reached`, `rate_limited`, `plan_required`) e `Message`.

---

## Sobre o autor

**Weslley Harakawa** - Fundador da ORYKSA AI. Em Lisboa, Portugal. Especialidades: inteligência artificial, desenvolvimento web e mobile, tokenização em blockchain. Formação: University of the People.

- LinkedIn: https://www.linkedin.com/in/weslleyharakawa/
- ORYKSA AI Employees: https://oryksa.com (X: https://x.com/oryksa, Instagram: https://www.instagram.com/oryksaai, YouTube: https://www.youtube.com/@ORYKSAAI)

---

MIT License · ORYKSA AI Employees · W8 Atlantic Unipessoal Lda
