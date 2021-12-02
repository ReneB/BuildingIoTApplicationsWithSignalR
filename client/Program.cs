using Microsoft.AspNetCore.SignalR.Client;

static async Task Ping(HubConnection connection, string deviceId) {
    await connection.SendAsync(nameof(Ping), deviceId);
}

int pingDelayInSeconds = 30;
int initialRetryDelayInSeconds = 5;

var identifier = Guid.NewGuid().ToString();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/main")
    .WithAutomaticReconnect()
    .Build();

while (connection.State != HubConnectionState.Connected) {
    try {
        await connection.StartAsync();
    } catch (HttpRequestException) {
        Console.WriteLine($"Failed to establish connection, will retry in {initialRetryDelayInSeconds} seconds");
        await Task.Delay(initialRetryDelayInSeconds * 1000);
    }
}

Console.WriteLine("Connection established");

connection.On<string, string, byte[]>(nameof(Client.Messages.Announce), async (announcementId, announcementText, byteArrayMp3) => {
    Console.WriteLine($"Receiving announcement: {announcementText}");

    var fileName = $"Temp/{announcementId}.mp3";
    await File.WriteAllBytesAsync(fileName, byteArrayMp3);

    Console.WriteLine($"Reassembled incoming announcement as {fileName}");

    var player = new NetCoreAudio.Player();
    await player.Play(fileName).WaitAsync(TimeSpan.FromSeconds(30));
});

while (true) {
    if (connection.State == HubConnectionState.Connected) {
        Console.WriteLine("Ping!");

        await Ping(connection, identifier);
    } else {
        Console.WriteLine("Connection down; skipping ping.");
    }

    await Task.Delay(pingDelayInSeconds * 1000);
}

namespace Client {
    public enum Messages {
        Announce
    }
}
