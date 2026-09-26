<p align="center"><img src="https://app.oryksa.com/static/oryksa_logo.png" width="300" alt="ORYKSA"></p>

# Oryksa for .NET

Official .NET SDK for **ORYKSA AI Employees**, for Windows and Linux desktop apps (WPF, WinUI, Avalonia, MAUI), servers (ASP.NET) and workers. The same AI employee that answers your customers on WhatsApp and on your website, inside your app.

* Docs: https://developer.oryksa.com/en/sdks
* API: `https://api.oryksa.com/v1`
* netstandard2.0 and net8.0, no dependencies on .NET 8.

## Install

```bash
dotnet add package Oryksa
```

## How it works (and why the key stays safe)

1. Your **server** keeps the secret API key (`oryk_live_...`, created at developer.oryksa.com) and creates a short-lived **session token** for each signed-in user.
2. Your **app** receives only that session token (`oryk_cs_...`). The secret key never goes into the app.
3. Every AI reply counts as one interaction of your ORYKSA plan, the same as on WhatsApp.

## In the app

```csharp
using Oryksa;

var client = new OryksaClient(getToken: async ct => await MyBackend.FetchOryksaTokenAsync(ct)); // "oryk_cs_..."
var agent = await client.AgentAsync();                 // name, photo, greeting, suggestions
var reply = await client.SendAndWaitAsync("Are you open on Saturday?");
```

Build your chat window with `agent.Name`, `agent.Avatar` (the name and photo from "Your AI" in ORYKSA) and `OryksaAgent.Pick(agent.Greeting, "en")`. For a ready chat with the ORYKSA look in a desktop app, host the website chat in a WebView (WebView2 or CefSharp) or use the JavaScript SDK in Electron or Tauri.

## On your server (secret key)

```csharp
var oryksa = new OryksaServer(Environment.GetEnvironmentVariable("ORYKSA_API_KEY")!);
var session = await oryksa.CreateSessionAsync(conversationId: "user-42", customerName: "Ana");
// send only session.GetProperty("client_token") to the app

await oryksa.SetPagesAsync(new[] { new Dictionary<string, string> { ["title"] = "Workouts", ["content"] = "Tap + to log a workout." } });
```

## Webhooks

```csharp
var evt = OryksaWebhook.Verify(rawBody, Request.Headers["ORYKSA-Signature"], webhookSecret);
```

## Errors

Failed calls throw `OryksaException` with `Status`, `Code` (for example `interaction_limit_reached`, `rate_limited`, `plan_required`) and `Message`.

---

## Português (Portugal)

SDK oficial .NET da **ORYKSA AI Employees** para apps desktop Windows e Linux e servidores. O teu servidor cria o token de sessão com `OryksaServer.CreateSessionAsync` e a app usa `OryksaClient(getToken: ...)`. Cada resposta da IA conta como uma interação do teu plano.

## Português (Brasil)

SDK oficial .NET da **ORYKSA AI Employees** para apps desktop Windows e Linux e servidores. Seu servidor cria o token de sessão com `OryksaServer.CreateSessionAsync` e o app usa `OryksaClient(getToken: ...)`. Cada resposta da IA conta como uma interação do seu plano.

## Español

SDK oficial de .NET de **ORYKSA AI Employees** para apps de escritorio Windows y Linux y servidores. Tu servidor crea el token de sesión con `OryksaServer.CreateSessionAsync` y la app usa `OryksaClient(getToken: ...)`. Cada respuesta de la IA cuenta como una interacción de tu plan.

---

MIT License · ORYKSA AI Employees · W8 Atlantic Unipessoal Lda
