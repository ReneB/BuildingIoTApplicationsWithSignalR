using Microsoft.AspNetCore.SignalR.Client;

static async Task Ping(HubConnection connection, string deviceId) {
    await connection.SendAsync(nameof(Ping), deviceId);
}

int pingDelayInSeconds = 30;

var identifier = Guid.NewGuid().ToString();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/main")
    .WithAutomaticReconnect()
    .Build();

await connection.StartAsync();

Console.WriteLine("Connection established");

while (true) {
    if (connection.State == HubConnectionState.Connected) {
        Console.WriteLine("Ping!");

        await Ping(connection, identifier);
    } else {
        Console.WriteLine("Connection down; skipping ping.");
    }

    await Task.Delay(pingDelayInSeconds * 1000);
}