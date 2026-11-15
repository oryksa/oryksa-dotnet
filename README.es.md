<p align="center"><img src="https://app.oryksa.com/static/oryksa_logo.png" width="300" alt="ORYKSA"></p>

# Oryksa for .NET

[![English](https://img.shields.io/badge/English-30363d?style=for-the-badge)](README.md) [![Português](https://img.shields.io/badge/Portugu%C3%AAs-30363d?style=for-the-badge)](README.pt.md) [![Español](https://img.shields.io/badge/Espa%C3%B1ol-0891b2?style=for-the-badge)](README.es.md) [![Français](https://img.shields.io/badge/Fran%C3%A7ais-30363d?style=for-the-badge)](README.fr.md)

SDK oficial de .NET de **ORYKSA AI Employees**, para apps de escritorio Windows y Linux (WPF, WinUI, Avalonia, MAUI), servidores (ASP.NET) y workers. El mismo empleado de IA que responde a tus clientes en WhatsApp y en tu web, dentro de tu app.

* Documentación: https://developer.oryksa.com/es/sdks
* API: `https://api.oryksa.com/v1`
* netstandard2.0 y net8.0, sin dependencias en .NET 8.

## Instalar

```bash
dotnet add package Oryksa
```

## Cómo funciona (y por qué la clave está segura)

1. Tu **servidor** guarda la clave secreta de la API (`oryk_live_...`, creada en developer.oryksa.com) y crea un **token de sesión** de corta duración para cada usuario que ha iniciado sesión.
2. Tu **app** recibe solo ese token de sesión (`oryk_cs_...`). La clave secreta nunca entra en la app.
3. Cada respuesta de la IA cuenta como una interacción de tu plan ORYKSA, igual que en WhatsApp.

## En la app

```csharp
using Oryksa;

var client = new OryksaClient(getToken: async ct => await MyBackend.FetchOryksaTokenAsync(ct)); // "oryk_cs_..."
var agent = await client.AgentAsync();                 // nombre, foto, saludo, sugerencias
var reply = await client.SendAndWaitAsync("Are you open on Saturday?");
```

Crea tu ventana de chat con `agent.Name`, `agent.Avatar` (el nombre y la foto de "Tu IA" en ORYKSA) y `OryksaAgent.Pick(agent.Greeting, "en")`. Para tener un chat listo con el aspecto de ORYKSA en una app de escritorio, integra el chat de la web en un WebView (WebView2 o CefSharp) o usa el SDK de JavaScript en Electron o Tauri.

## En tu servidor (clave secreta)

```csharp
var oryksa = new OryksaServer(Environment.GetEnvironmentVariable("ORYKSA_API_KEY")!);
var session = await oryksa.CreateSessionAsync(conversationId: "user-42", customerName: "Ana");
// envía a la app solo session.GetProperty("client_token")

await oryksa.SetPagesAsync(new[] { new Dictionary<string, string> { ["title"] = "Workouts", ["content"] = "Tap + to log a workout." } });
```

## Webhooks

```csharp
var evt = OryksaWebhook.Verify(rawBody, Request.Headers["ORYKSA-Signature"], webhookSecret);
```

## Errores

Las llamadas que fallan lanzan `OryksaException` con `Status`, `Code` (por ejemplo `interaction_limit_reached`, `rate_limited`, `plan_required`) y `Message`.

---

## Sobre el autor

**Weslley Harakawa** - Fundador de ORYKSA AI. En Lisboa, Portugal. Especialidades: inteligencia artificial, desarrollo web y móvil, tokenización en blockchain. Formación: University of the People.

- LinkedIn: https://www.linkedin.com/in/weslleyharakawa/
- ORYKSA AI Employees: https://oryksa.com (X: https://x.com/oryksa, Instagram: https://www.instagram.com/oryksaai, YouTube: https://www.youtube.com/@ORYKSAAI)

---

MIT License · ORYKSA AI Employees · W8 Atlantic Unipessoal Lda
