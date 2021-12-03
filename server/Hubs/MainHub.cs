using Microsoft.AspNetCore.SignalR;
using Server.Hubs.Services;

namespace Server.Hubs {
    public interface IDevice {
        Task Announce(string announcementId, string announcementText, byte[] announcementMp3);
        Task RegisterDevicePresence(string deviceId, DateTime timestamp);
        Task RegisterDeviceOffline(string deviceId);
    }

    public class MainHub : Hub<IDevice> {
        private const string groupNameForMasters = "Masters";
        private readonly ConnectedClientRegistry clientRegistry;

        public MainHub(ConnectedClientRegistry clientRegistry) {
            this.clientRegistry = clientRegistry;
        }

        protected IDevice Masters {
            get {
                return Clients.Group(groupNameForMasters);
            }
        }

        public async Task Ping() {
            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);

            if (deviceId != null) {
                await Masters.RegisterDevicePresence(deviceId, DateTime.UtcNow);
            }
        }

        public void RegisterDevice(string deviceId, int gateNumber) {
            clientRegistry.Register(Context.ConnectionId, deviceId);
        }

        public async Task RegisterAsMaster() {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupNameForMasters);
        }

        public override async Task OnDisconnectedAsync(Exception? exception) {
            var deviceId = clientRegistry.FindDeviceId(Context.ConnectionId);

            if (deviceId != default) {
                await Masters.RegisterDeviceOffline(deviceId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}