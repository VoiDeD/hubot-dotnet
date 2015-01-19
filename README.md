## Hubot.NET
A Keep-It-Simple-Stupid async (TPL) .NET library and Hubot adapter.

Hubot.NET is the .NET library portion, and is designed to help you quickly and easily interact with a Hubot instance from an application via IPC, namely via TCP sockets. The library is designed with the async/await pattern in mind and allows you to make your interactions completely asynchronous if the need arises.

##### Dependencies
* [Nito.AsyncEx](https://www.nuget.org/packages/Nito.AsyncEx), which should be automatically installed by NuGet when building.

##### Usage
```csharp

async Task Init()
{
    var client = new HubotClient();
    
    // hook up the events we care about
    client.Chat += OnChat;
    client.Disconnected += OnDisconnected;

    // connect to the dotnet adapter
    await client.Connect( "localhost", 8880 );

    // pretend that User123 is really hungry right now
    await client.SendChat( "User123", "hubot: animate me a pizza" );
}

async void OnDisconnected( object sender, DisconnectEventArgs e )
{
    Console.WriteLine( "Disconnected from Hubot adapter!" );
    
    // add your reconnection logic  here
}

void OnChat( object sender, MessageEventArgs e )
{
    Console.WriteLine( "Hubot says: {0}", e.Message );
    
    // you'd want to pipe this message back to where the source message originally came from
}
```

## hubot-dotnet
hubot-dotnet is the Hubot adapter portion, and is designed as a simple TCP server that routes chat to and from the Hubot instance.

##### Dependencies
* [buffercursor](https://www.npmjs.com/package/buffercursor), which should be automatically retrieved by `npm install`.

##### Usage
1. Set `HUBOT_DOTNET_PORT` environment variable to the tcp port (default is 8880) the dotnet adapter will listen on.
2. In your Hubot's directory: `npm install "<Path To hubot-dotnet>"`.
3. Launch your Hubot with `--adapter dotnet`.

## Considerations
* The Hubot adapter itself performs no access control for which clients may connect, this should be performed at the firewall/iptables level.
* Traffic is sent entirely in plaintext between the .NET library and the adapter. There is no TLS or other crypto support.
