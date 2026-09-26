using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Oryksa;
using Xunit;

public class OryksaTests
{
    [Fact]
    public void AgentParsingAndLanguageFallbacks()
    {
        var a = JsonSerializer.Deserialize<OryksaAgent>("{\"name\":\"ORYKSA\",\"avatar\":\"https://x/a.jpg\",\"greeting\":{\"en\":\"Hi\",\"pt\":\"Olá\"},\"suggestions\":{\"en\":[\"Prices?\"]},\"voice_replies\":true}")!;
        Assert.Equal("ORYKSA", a.Name);
        Assert.True(a.VoiceReplies);
        Assert.Equal("Olá", OryksaAgent.Pick(a.Greeting, "br"));
        Assert.Equal("Hi", OryksaAgent.Pick(a.Greeting, "es"));
        Assert.Equal("Prices?", OryksaAgent.Pick(a.Suggestions, "pt")![0]);
    }

    [Fact]
    public void SecretKeyIsRefusedInTheAppClient()
    {
        Assert.Throws<ArgumentException>(() => new OryksaClient(token: "oryk_live_abc"));
        Assert.Throws<ArgumentException>(() => new OryksaClient());
        Assert.Throws<ArgumentException>(() => new OryksaServer("oryk_cs_abc"));
    }

    [Fact]
    public void WebhookSignature()
    {
        const string secret = "whsec_test";
        const string body = "{\"type\":\"message.replied\"}";
        var t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var sig = BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(t + "." + body))).Replace("-", "").ToLowerInvariant();
        Assert.Equal("message.replied", OryksaWebhook.Verify(body, $"t={t},v1={sig}", secret).GetProperty("type").GetString());
        Assert.Throws<OryksaException>(() => OryksaWebhook.Verify(body, $"t={t},v1=00", secret));
    }
}
