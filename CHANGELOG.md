# Changelog

## 1.0.0

* First release.
* `OryksaClient`: in-app client for desktop and mobile .NET apps with short-lived session tokens (AgentAsync, SendAsync, SendAndWaitAsync, MessagesAsync), refreshes the token when it expires.
* `OryksaServer`: server client with the secret key (ChatAsync, CreateSessionAsync, SetPagesAsync, AddFaqAsync).
* `OryksaWebhook.Verify`: webhook signature check.
* netstandard2.0 and net8.0.
