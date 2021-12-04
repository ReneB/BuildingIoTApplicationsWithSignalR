using Microsoft.AspNetCore.SignalR;
using Server.Hubs.Services;
using Server.Services;

namespace Server.Hubs {
    public interface IDevice {
        Task Announce(string announcementId, string announcementText, byte[] announcementMp3);
        Task RegisterDevicePresence(string deviceId, DateTime timestamp);
        Task RegisterDeviceOffline(string deviceId);
    }

    public class MainHub : Hub<IDevice> {
        private const string groupNameForMasters = "Masters";
        private readonly ConnectedClientRegistry clientRegistry;
        private readonly GateToGroupMap gateToGroupMap;

        public MainHub(ConnectedClientRegistry clientRegistry, GateToGroupMap gateToGroupMap) {
            this.clientRegistry = clientRegistry;
            this.gateToGroupMap = gateToGroupMap;
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

        public async Task RegisterDevice(string deviceId, int gateNumber) {
            clientRegistry.Register(Context.ConnectionId, deviceId);

            var group = gateToGroupMap.GroupForGate(gateNumber);

            await Groups.AddToGroupAsync(Context.ConnectionId, group);
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