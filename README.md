Hubot.NET
---
A Keep-It-Simple-Stupid async (TPL) .NET library and Hubot adapter.

### Hubot.NET
Hubot.NET is the .NET library portion, and is designed to help you quickly and easily interact with a Hubot instance from an application via IPC, namely via TCP sockets. The library is designed with the async/await pattern in mind and allows you to make your interactions completely asynchronous if the need arises.

##### Usage
```csharp
HubotClient client = new HubotClient();

...

async Task Init()
{
    client.Chat += OnChat;
    client.Disconnected += OnDisconnected;

    await client.Connect( "localhost", 1234 );

    await client.SendChat( "User123", "hubot: animate me a pizza" );
}

void OnDisconnected( object sender, DisconnectEventArgs e )
{
    Console.WriteLine( "Disconnected from Hubot adapter!" );
}

void OnChat( object sender, MessageEventArgs e )
{
    Console.WriteLine( "Hubot says: {0}", e.Message );
}
```

### hubot-dotnet
hubot-dotnet is the Hubot adapter portion, and is designed as a simple TCP server that routes chat to and from the Hubot instance.
