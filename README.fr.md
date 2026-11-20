<p align="center"><img src="https://app.oryksa.com/static/oryksa_logo.png" width="300" alt="ORYKSA"></p>

# Oryksa for .NET

[![English](https://img.shields.io/badge/English-30363d?style=for-the-badge)](README.md) [![Português](https://img.shields.io/badge/Portugu%C3%AAs-30363d?style=for-the-badge)](README.pt.md) [![Español](https://img.shields.io/badge/Espa%C3%B1ol-30363d?style=for-the-badge)](README.es.md) [![Français](https://img.shields.io/badge/Fran%C3%A7ais-0891b2?style=for-the-badge)](README.fr.md)

SDK .NET officiel d'**ORYKSA AI Employees**, pour les apps de bureau Windows et Linux (WPF, WinUI, Avalonia, MAUI), les serveurs (ASP.NET) et les workers. Le même employé IA qui répond à vos clients sur WhatsApp et sur votre site web, dans votre app.

* Documentation : https://developer.oryksa.com/fr/sdks
* API : `https://api.oryksa.com/v1`
* netstandard2.0 et net8.0, sans dépendances sur .NET 8.

## Installer

```bash
dotnet add package Oryksa
```

## Comment ça marche (et pourquoi la clé reste en sécurité)

1. Votre **serveur** garde la clé API secrète (`oryk_live_...`, créée sur developer.oryksa.com) et crée un **jeton de session** de courte durée pour chaque utilisateur connecté.
2. Votre **app** ne reçoit que ce jeton de session (`oryk_cs_...`). La clé secrète n'entre jamais dans l'app.
3. Chaque réponse de l'IA compte comme une interaction de votre forfait ORYKSA, comme sur WhatsApp.

## Dans l'app

```csharp
using Oryksa;

var client = new OryksaClient(getToken: async ct => await MyBackend.FetchOryksaTokenAsync(ct)); // "oryk_cs_..."
var agent = await client.AgentAsync();                 // nom, photo, message d'accueil, suggestions
var reply = await client.SendAndWaitAsync("Are you open on Saturday?");
```

Construisez votre fenêtre de chat avec `agent.Name`, `agent.Avatar` (le nom et la photo de "Votre IA" dans ORYKSA) et `OryksaAgent.Pick(agent.Greeting, "en")`. Pour un chat prêt à l'emploi avec le style ORYKSA dans une app de bureau, intégrez le chat du site web dans une WebView (WebView2 ou CefSharp) ou utilisez le SDK JavaScript dans Electron ou Tauri.

## Sur votre serveur (clé secrète)

```csharp
var oryksa = new OryksaServer(Environment.GetEnvironmentVariable("ORYKSA_API_KEY")!);
var session = await oryksa.CreateSessionAsync(conversationId: "user-42", customerName: "Ana");
// envoyez uniquement session.GetProperty("client_token") à l'app

await oryksa.SetPagesAsync(new[] { new Dictionary<string, string> { ["title"] = "Workouts", ["content"] = "Tap + to log a workout." } });
```

## Webhooks

```csharp
var evt = OryksaWebhook.Verify(rawBody, Request.Headers["ORYKSA-Signature"], webhookSecret);
```

## Erreurs

Les appels qui échouent lèvent `OryksaException` avec `Status`, `Code` (par exemple `interaction_limit_reached`, `rate_limited`, `plan_required`) et `Message`.

---

## À propos de l'auteur

**Weslley Harakawa** - Fondateur d'ORYKSA AI. À Lisbonne, Portugal. Spécialités : intelligence artificielle, développement web et mobile, tokenisation blockchain. Formation : University of the People.

- LinkedIn: https://www.linkedin.com/in/weslleyharakawa/
- ORYKSA AI Employees: https://oryksa.com (X: https://x.com/oryksa, Instagram: https://www.instagram.com/oryksaai, YouTube: https://www.youtube.com/@ORYKSA)

---

MIT License · ORYKSA AI Employees · W8 Atlantic Unipessoal Lda
