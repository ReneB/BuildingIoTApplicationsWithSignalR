using Microsoft.AspNetCore.SignalR.Client;

int pingDelayInSeconds = 30;

var identifier = Guid.NewGuid().ToString();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/main")
    .WithAutomaticReconnect()
    .Build();

await connection.StartAsync();

Console.WriteLine("Connection established");

while (true) {
    await Task.Delay(pingDelayInSeconds * 1000);

    if (connection.State == HubConnectionState.Connected) {
        Console.WriteLine("Ping!");

        await connection.SendAsync("Ping", identifier);
    } else {
        Console.WriteLine("Connection down; skipping ping.");
    }
}