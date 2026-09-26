// Minimal ORYKSA client in a .NET desktop or console app.
// Your server returns {"token": "oryk_cs_..."} for the signed-in user (POST /v1/sessions with the secret key).
using System.Net.Http.Json;
using Oryksa;

var http = new HttpClient();
var client = new OryksaClient(getToken: async ct =>
    (await http.GetFromJsonAsync<Dictionary<string, string>>("https://your-server.example/oryksa-token", ct))!["token"]);

var agent = await client.AgentAsync();
Console.WriteLine($"{agent.Name}: {OryksaAgent.Pick(agent.Greeting, "en")}");
while (Console.ReadLine() is string line && line.Length > 0)
{
    Console.WriteLine($"{agent.Name}: {await client.SendAndWaitAsync(line)}");
}
