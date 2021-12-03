using Microsoft.AspNetCore.SignalR.Client;

await new Client().Start();

public class Client {
    private const int pingDelayInSeconds = 30;
    private const int initialRetryDelayInSeconds = 5;

    private HubConnection connection;
    private readonly string identifier = Guid.NewGuid().ToString();
    private string url = "https://localhost:5001/main";

    public Client() {
        connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task Start() {
        Console.WriteLine($"Current client id: {identifier}");

        AddEventListeners();

        await Connect();

        await KeepAlive();
    }

    private async Task Connect() {
        while (connection.State != HubConnectionState.Connected) {
            try {
                await connection.StartAsync();

                await RegisterDevice();
            } catch (HttpRequestException) {
                Console.WriteLine($"Failed to establish connection, will retry in {initialRetryDelayInSeconds} seconds");
                await Task.Delay(initialRetryDelayInSeconds * 1000);
            }
        }

        Console.WriteLine("Connection established");
    }

    private async Task KeepAlive() {
        while (true) {
            if (connection.State == HubConnectionState.Connected) {
                Console.WriteLine("Ping!");

                await Ping();
            } else {
                Console.WriteLine("Connection down; skipping ping.");
            }

            await Task.Delay(pingDelayInSeconds * 1000);
        }
    }

    public void AddEventListeners() {
        connection.On<string, string, byte[]>(nameof(Announce), Announce);
        connection.Reconnected += async (_) => await RegisterDevice();
    }

    private async Task Announce(string announcementId, string announcementText, byte[] byteArrayMp3) {
        Console.WriteLine($"Receiving announcement: {announcementText}");

        var fileName = $"Temp/{announcementId}.mp3";
        await File.WriteAllBytesAsync(fileName, byteArrayMp3);

        Console.WriteLine($"Reassembled incoming announcement as {fileName}");

        var player = new NetCoreAudio.Player();
        await player.Play(fileName).WaitAsync(TimeSpan.FromSeconds(30));
    }

    private async Task Ping() {
        await connection.SendAsync(nameof(Ping));
    }

    private async Task RegisterDevice() {
        await connection.SendAsync(nameof(RegisterDevice), identifier);
    }
}