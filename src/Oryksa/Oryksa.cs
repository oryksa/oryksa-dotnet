// ORYKSA AI Employees SDK for .NET. License: MIT. Docs: https://developer.oryksa.com
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Oryksa
{
    /// <summary>Error returned by the ORYKSA API. <see cref="Code"/> is stable, for example
    /// <c>interaction_limit_reached</c>, <c>rate_limited</c>, <c>plan_required</c> or <c>session_expired</c>.</summary>
    public sealed class OryksaException : Exception
    {
        /// <summary>HTTP status (0 when the request did not reach the server).</summary>
        public int Status { get; }
        /// <summary>Machine readable code.</summary>
        public string Code { get; }
        /// <summary>Creates the error.</summary>
        public OryksaException(int status, string code, string message) : base(message) { Status = status; Code = code; }
    }

    /// <summary>Public look of the AI employee (the same data the ORYKSA website chat shows).</summary>
    public sealed class OryksaAgent
    {
        /// <summary>Name of the AI employee (from "Your AI" in ORYKSA).</summary>
        [JsonPropertyName("name")] public string Name { get; set; } = "ORYKSA";
        /// <summary>Photo of the AI employee (from "Your AI" in ORYKSA).</summary>
        [JsonPropertyName("avatar")] public string Avatar { get; set; } = "https://oryksa.com/assets/img/avatar_official_oryksa.png";
        /// <summary>Business name.</summary>
        [JsonPropertyName("business")] public string? Business { get; set; }
        /// <summary>Greeting per language (en, pt, br, es).</summary>
        [JsonPropertyName("greeting")] public Dictionary<string, string> Greeting { get; set; } = new Dictionary<string, string>();
        /// <summary>Subtitle per language.</summary>
        [JsonPropertyName("subtitle")] public Dictionary<string, string> Subtitle { get; set; } = new Dictionary<string, string>();
        /// <summary>Suggested questions per language.</summary>
        [JsonPropertyName("suggestions")] public Dictionary<string, List<string>> Suggestions { get; set; } = new Dictionary<string, List<string>>();
        /// <summary>The plan allows voice replies.</summary>
        [JsonPropertyName("voice_replies")] public bool VoiceReplies { get; set; }
        /// <summary>Conversation of this session.</summary>
        [JsonPropertyName("conversation_id")] public string? ConversationId { get; set; }

        /// <summary>Picks the value for <paramref name="lang"/> with fallbacks (br uses pt, then en).</summary>
        public static T? Pick<T>(IDictionary<string, T> map, string lang) where T : class
        {
            if (map.TryGetValue(lang, out var v)) return v;
            if (lang == "br" && map.TryGetValue("pt", out var p)) return p;
            if (map.TryGetValue("en", out var e)) return e;
            return map.Values.FirstOrDefault();
        }
    }

    /// <summary>One message of the conversation.</summary>
    public sealed class OryksaMessage
    {
        /// <summary><c>user</c> or <c>assistant</c>.</summary>
        [JsonPropertyName("role")] public string Role { get; set; } = "assistant";
        /// <summary>Text.</summary>
        [JsonPropertyName("content")] public string Content { get; set; } = "";
    }

    /// <summary>Answer of a chat call: <c>replied</c> with the text, or <c>pending</c> while the AI is still writing.</summary>
    public sealed class OryksaReply
    {
        /// <summary><c>replied</c> or <c>pending</c>.</summary>
        [JsonPropertyName("status")] public string Status { get; set; } = "replied";
        /// <summary>Reply text when replied.</summary>
        [JsonPropertyName("reply")] public string? Reply { get; set; }
        /// <summary>Conversation id.</summary>
        [JsonPropertyName("conversation_id")] public string? ConversationId { get; set; }
    }

    internal static class Http
    {
        public const string Version = "1.0.0";
        public static readonly JsonSerializerOptions Json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static async Task<string> SendAsync(HttpClient http, string baseUrl, string token, HttpMethod method, string path, object? body, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(method, baseUrl + path);
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            req.Headers.TryAddWithoutValidation("X-ORYKSA-SDK", "dotnet/" + Version);
            req.Headers.TryAddWithoutValidation("Accept", "application/json");
            if (body != null) req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            HttpResponseMessage res;
            try { res = await http.SendAsync(req, ct).ConfigureAwait(false); }
            catch (HttpRequestException e) { throw new OryksaException(0, "network_error", "Could not reach ORYKSA: " + e.Message); }
            using (res)
            {
                var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                var status = (int)res.StatusCode;
                if (status >= 400)
                {
                    string code = "http_" + status, msg = "Request failed with HTTP " + status;
                    try
                    {
                        using var doc = JsonDocument.Parse(text);
                        if (doc.RootElement.TryGetProperty("error", out var e))
                        {
                            if (e.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String) code = c.GetString()!;
                            if (e.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) msg = m.GetString()!;
                        }
                    }
                    catch (JsonException) { }
                    throw new OryksaException(status, code, msg);
                }
                return text;
            }
        }
    }

    /// <summary>In-app client for desktop and mobile apps. Uses a short-lived session token (oryk_cs_...) created by YOUR
    /// server with POST /v1/sessions. The secret API key never goes into the app.</summary>
    public sealed class OryksaClient
    {
        private readonly HttpClient _http;
        private readonly string _base;
        private readonly Func<CancellationToken, Task<string>>? _getToken;
        private string? _token;

        /// <summary>Creates the client with a token, a getToken callback, or both.</summary>
        public OryksaClient(string? token = null, Func<CancellationToken, Task<string>>? getToken = null,
            string baseUrl = "https://api.oryksa.com/v1", HttpClient? httpClient = null)
        {
            if (token == null && getToken == null) throw new ArgumentException("OryksaClient needs a token or getToken.");
            if (token != null && token.StartsWith("oryk_live_", StringComparison.Ordinal))
                throw new ArgumentException("Never use the secret API key in an app. Use a session token (oryk_cs_...).");
            _token = token; _getToken = getToken; _base = baseUrl.TrimEnd('/'); _http = httpClient ?? new HttpClient();
        }

        private async Task<string> TokenAsync(bool force, CancellationToken ct)
        {
            if ((_token == null || force) && _getToken != null) _token = await _getToken(ct).ConfigureAwait(false);
            if (string.IsNullOrEmpty(_token)) throw new OryksaException(401, "no_token", "No session token.");
            return _token!;
        }

        private async Task<T> RequestAsync<T>(HttpMethod method, string path, object? body, CancellationToken ct)
        {
            string text;
            try { text = await Http.SendAsync(_http, _base, await TokenAsync(false, ct).ConfigureAwait(false), method, path, body, ct).ConfigureAwait(false); }
            catch (OryksaException e) when ((e.Code == "session_expired" || e.Status == 401) && _getToken != null)
            { text = await Http.SendAsync(_http, _base, await TokenAsync(true, ct).ConfigureAwait(false), method, path, body, ct).ConfigureAwait(false); }
            return JsonSerializer.Deserialize<T>(string.IsNullOrEmpty(text) ? "{}" : text, Http.Json)!;
        }

        /// <summary>Name, photo, greeting and suggestions of the AI employee.</summary>
        public Task<OryksaAgent> AgentAsync(CancellationToken ct = default) => RequestAsync<OryksaAgent>(HttpMethod.Get, "/client/agent", null, ct);

        /// <summary>Sends a message. Status is <c>pending</c> when the AI needs a few more seconds: use <see cref="SendAndWaitAsync"/>.</summary>
        public Task<OryksaReply> SendAsync(string message, CancellationToken ct = default) =>
            RequestAsync<OryksaReply>(HttpMethod.Post, "/client/chat", new { message }, ct);

        private sealed class MessagesWrap { [JsonPropertyName("messages")] public List<OryksaMessage>? Messages { get; set; } }

        /// <summary>Messages of this conversation.</summary>
        public async Task<IReadOnlyList<OryksaMessage>> MessagesAsync(CancellationToken ct = default) =>
            (await RequestAsync<MessagesWrap>(HttpMethod.Get, "/client/messages", null, ct).ConfigureAwait(false)).Messages ?? new List<OryksaMessage>();

        /// <summary>Sends a message and waits for the reply text.</summary>
        public async Task<string?> SendAndWaitAsync(string message, TimeSpan? maxWait = null, CancellationToken ct = default)
        {
            var r = await SendAsync(message, ct).ConfigureAwait(false);
            if (r.Status != "pending") return r.Reply;
            var end = DateTime.UtcNow + (maxWait ?? TimeSpan.FromSeconds(40));
            while (DateTime.UtcNow < end)
            {
                await Task.Delay(1500, ct).ConfigureAwait(false);
                var last = (await MessagesAsync(ct).ConfigureAwait(false)).LastOrDefault();
                if (last?.Role == "assistant") return last.Content;
            }
            return null;
        }
    }

    /// <summary>Server client (ASP.NET, workers). Uses the SECRET API key (oryk_live_...). Never ship it inside an app.</summary>
    public sealed class OryksaServer
    {
        private readonly HttpClient _http;
        private readonly string _base;
        private readonly string _key;

        /// <summary>Creates the server client.</summary>
        public OryksaServer(string apiKey, string baseUrl = "https://api.oryksa.com/v1", HttpClient? httpClient = null)
        {
            if (apiKey == null || !apiKey.StartsWith("oryk_live_", StringComparison.Ordinal))
                throw new ArgumentException("An ORYKSA API key (oryk_live_...) is required. Create one at developer.oryksa.com.");
            _key = apiKey; _base = baseUrl.TrimEnd('/'); _http = httpClient ?? new HttpClient();
        }

        private async Task<T> RequestAsync<T>(HttpMethod m, string path, object? body, CancellationToken ct)
        {
            var text = await Http.SendAsync(_http, _base, _key, m, path, body, ct).ConfigureAwait(false);
            return JsonSerializer.Deserialize<T>(string.IsNullOrEmpty(text) ? "{}" : text, Http.Json)!;
        }

        /// <summary>Chat with the AI employee. Reuse conversationId to keep the context.</summary>
        public Task<OryksaReply> ChatAsync(string message, string? conversationId = null, string? customerName = null, CancellationToken ct = default) =>
            RequestAsync<OryksaReply>(HttpMethod.Post, "/chat", new Dictionary<string, object?> { ["message"] = message, ["conversation_id"] = conversationId, ["customer_name"] = customerName }, ct);

        /// <summary>Short-lived token for one app user. Send only client_token to the app.</summary>
        public Task<JsonElement> CreateSessionAsync(string? conversationId = null, string? customerName = null, int ttlMinutes = 60, CancellationToken ct = default) =>
            RequestAsync<JsonElement>(HttpMethod.Post, "/sessions", new Dictionary<string, object?> { ["conversation_id"] = conversationId, ["customer_name"] = customerName, ["ttl_minutes"] = ttlMinutes }, ct);

        /// <summary>Replace the app pages the AI knows (Brain). Each page: title, content and optional url.</summary>
        public Task<JsonElement> SetPagesAsync(IEnumerable<IDictionary<string, string>> pages, string? businessSummary = null, CancellationToken ct = default) =>
            RequestAsync<JsonElement>(new HttpMethod("PUT"), "/knowledge/pages", new Dictionary<string, object?> { ["pages"] = pages, ["business_summary"] = businessSummary }, ct);

        /// <summary>Add questions and answers to the Brain.</summary>
        public Task<JsonElement> AddFaqAsync(IEnumerable<IDictionary<string, string>> items, CancellationToken ct = default) =>
            RequestAsync<JsonElement>(HttpMethod.Post, "/knowledge/faq", new Dictionary<string, object?> { ["items"] = items }, ct);
    }

    /// <summary>ORYKSA webhook signature check.</summary>
    public static class OryksaWebhook
    {
        /// <summary>Verifies the ORYKSA-Signature header against the RAW body. Returns the parsed event or throws <see cref="OryksaException"/>.</summary>
        public static JsonElement Verify(string rawBody, string? signatureHeader, string secret, int toleranceSeconds = 300)
        {
            var parts = (signatureHeader ?? "").Split(',').Select(p => p.Split(new[] { '=' }, 2)).Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
            long.TryParse(parts.TryGetValue("t", out var ts) ? ts : "0", out var t);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (t == 0 || Math.Abs(now - t) > toleranceSeconds) throw new OryksaException(400, "invalid_signature", "Webhook timestamp is missing or too old.");
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var expected = BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(t + "." + rawBody))).Replace("-", "").ToLowerInvariant();
            var got = parts.TryGetValue("v1", out var v) ? v : "";
            var diff = expected.Length ^ got.Length;
            for (int i = 0; i < Math.Min(expected.Length, got.Length); i++) diff |= expected[i] ^ got[i];
            if (diff != 0) throw new OryksaException(400, "invalid_signature", "Webhook signature does not match.");
            using var doc = JsonDocument.Parse(rawBody);
            return doc.RootElement.Clone();
        }
    }
}
