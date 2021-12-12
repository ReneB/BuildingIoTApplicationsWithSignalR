using Microsoft.AspNetCore.SignalR.Client;
using Debug;
using System;

await new Client.Client().Start();

namespace Client {
    public class Client {
        public record AudioPlayingRecord(string DeviceId, DateTime Timestamp);
        private readonly TimeSpan pingDelay = TimeSpan.FromSeconds(3);
        private readonly TimeSpan initialRetryDelay = TimeSpan.FromSeconds(5);
        private readonly TimeSpan maxAudioReservationTime = TimeSpan.FromMinutes(1);
        private readonly TimeSpan graceTimeBeforePlayingAudio = TimeSpan.FromSeconds(15);
        private bool isPlayingAudio = false;
        private readonly Queue<string> audioQueue = new();

        private readonly HubConnection connection;
        private readonly string identifier = GetDeviceIdentifier();
        private readonly int gateNumber = GetGateNumber();
        private readonly string url = GetHubUrl();
        private readonly List<AudioPlayingRecord> peersPlayingAudio = new();

        protected bool AudioIsBeingPlayed => peersPlayingAudio.Count > 0;

        public Client() {
            connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task Start() {
            DebugConsole.WriteLine($"Current client id: {identifier}");
            DebugConsole.WriteLine($"Gate number: {gateNumber}");
            DebugConsole.WriteLine($"Hub URL: {url}");

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
                    DebugConsole.WriteLine($"Failed to establish connection, will retry in {initialRetryDelay.Seconds} seconds");
                    await Task.Delay(initialRetryDelay);
                }
            }

            DebugConsole.WriteLine("Connection established");
        }

        private async Task KeepAlive() {
            while (true) {
                if (connection.State == HubConnectionState.Connected) {
                    await Ping();
                } else {
                    DebugConsole.WriteLine("Connection down; skipping ping.");
                }

                ClearLingeringAudioRegistrations();
                await PlayQueue();

                await Task.Delay(pingDelay);
            }
        }

        public void AddEventListeners() {
            connection.On<string, string, byte[]>(nameof(Announce), Announce);
            connection.On<string>(nameof(RegisterAudioStarted), RegisterAudioStarted);
            connection.On<string>(nameof(RegisterAudioEnded), RegisterAudioEnded);
            connection.Reconnected += async (_) => await RegisterDevice();
        }

        private async Task Announce(string announcementId, string announcementText, byte[] byteArrayMp3) {
            DebugConsole.WriteLine($"Receiving announcement: {announcementText}");

            var fileName = $"Temp/{announcementId}-{new Random().Next()}.mp3";
            await File.WriteAllBytesAsync(fileName, byteArrayMp3);

            DebugConsole.WriteLine($"Reassembled incoming announcement as {fileName}");

            audioQueue.Enqueue(fileName);
        }

        private async Task Play(string fileName) {
            if (isPlayingAudio || AudioIsBeingPlayed) {
                audioQueue.Enqueue(fileName);

                return;
            }

            isPlayingAudio = true;

            DebugConsole.WriteLine($"Playing audio: {fileName}");

            await NotifyAudioStarted();

            var player = new NetCoreAudio.Player();
            player.PlaybackFinished += async (sender, args) => await OnAudioFinished();

            await player.Play(fileName).WaitAsync(TimeSpan.FromSeconds(30));
        }

        private async Task OnAudioFinished() {
            isPlayingAudio = false;

            var fileName = audioQueue.Dequeue();
            File.Delete(fileName);

            await NotifyAudioEnded();
        }

        private async Task PlayQueue() {
            if (!isPlayingAudio && !AudioIsBeingPlayed && audioQueue.Count > 0) {
                await Play(audioQueue.Peek());
            }
        }

        private void ClearLingeringAudioRegistrations() {
            var cutoffTime = DateTime.Now.Subtract(maxAudioReservationTime);

            var numberOfStaleRemovals = peersPlayingAudio.RemoveAll(r => r.Timestamp < cutoffTime);

            if (numberOfStaleRemovals > 0) {
                DebugConsole.WriteLine($"Removed {numberOfStaleRemovals} from the list of peers playing audio; assuming connection dropped on their side.");
            }
        }

        private Task RegisterAudioStarted(string deviceId) {
            if (deviceId != identifier) {
                peersPlayingAudio.Add(new AudioPlayingRecord(deviceId, DateTime.Now));
            }

            return Task.CompletedTask;
        }

        private async Task RegisterAudioEnded(string deviceId) {
            if (deviceId != identifier) {
                peersPlayingAudio.RemoveAll(r => r.DeviceId == deviceId);
            }

            await PlayQueue();
        }

        private async Task NotifyAudioStarted() {
            await connection.SendAsync(nameof(NotifyAudioStarted), gateNumber);
        }

        private async Task NotifyAudioEnded() {
            await connection.SendAsync(nameof(NotifyAudioEnded), gateNumber);
        }

        private async Task Ping() {
            DebugConsole.WriteLine("Ping!");

            await connection.SendAsync(nameof(Ping));
        }

        private async Task RegisterDevice() {
            DebugConsole.WriteLine("Registering our presence with the server");

            await connection.SendAsync(nameof(RegisterDevice), identifier, gateNumber);
        }

        private static int GetGateNumber() {
            return int.TryParse(Environment.GetEnvironmentVariable("GATE_NUMBER"), out var gateNumber) ? gateNumber : RandomGateNumber();
        }

        private static string GetDeviceIdentifier() {
            var deviceId = Environment.GetEnvironmentVariable("DEVICE_ID");

            return string.IsNullOrEmpty(deviceId) ? Guid.NewGuid().ToString() : deviceId;
        }

        private static string GetHubUrl() {
            var url = Environment.GetEnvironmentVariable("HUB_URL");

            return string.IsNullOrEmpty(url) ? "https://localhost:5001/main" : url;
        }

        private static int RandomGateNumber() {
            return 1 + new Random().Next(10);
        }
    }
}